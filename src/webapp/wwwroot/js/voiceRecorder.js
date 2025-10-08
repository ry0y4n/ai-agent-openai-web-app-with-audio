const DEFAULT_SAMPLE_RATE = 16000;
let audioContext = null;
let mediaStream = null;
let mediaStreamSource = null;
let processor = null;
let recordingBuffers = [];
let recording = false;
let dotNetCallback = null;
let desiredSampleRate = DEFAULT_SAMPLE_RATE;

function resetRecordingState() {
  recordingBuffers = [];
  recording = false;
}

function invokeDotNet(methodName, ...args) {
  if (!dotNetCallback) {
    console.warn(
      `[voiceRecorder] DotNet callback is null when invoking ${methodName}`
    );
    return Promise.resolve();
  }

  try {
    const result = dotNetCallback.invokeMethodAsync(methodName, ...args);
    if (result && typeof result.catch === "function") {
      result.catch((err) => {
        console.error(
          `[voiceRecorder] invokeMethodAsync(${methodName}) rejected`,
          err
        );
      });
    }
    return result;
  } catch (error) {
    console.error(
      `[voiceRecorder] invokeMethodAsync(${methodName}) threw`,
      error
    );
    return Promise.reject(error);
  }
}

function notifyStatus(status) {
  console.debug(`[voiceRecorder] status -> ${status}`);
  invokeDotNet("OnVoiceRecorderStatusChanged", status);
}

function notifyError(message) {
  console.error(`[voiceRecorder] error -> ${message}`);
  invokeDotNet("OnVoiceRecorderError", message);
}

function mergeBuffers(buffers) {
  const totalLength = buffers.reduce((sum, buffer) => sum + buffer.length, 0);
  const result = new Float32Array(totalLength);
  let offset = 0;

  for (const buffer of buffers) {
    result.set(buffer, offset);
    offset += buffer.length;
  }

  return result;
}

function downsampleBuffer(buffer, originalRate, targetRate) {
  if (targetRate >= originalRate) {
    return buffer;
  }

  const sampleRateRatio = originalRate / targetRate;
  const newLength = Math.round(buffer.length / sampleRateRatio);
  const result = new Float32Array(newLength);
  let offsetResult = 0;
  let offsetBuffer = 0;

  while (offsetResult < result.length) {
    const nextOffsetBuffer = Math.round((offsetResult + 1) * sampleRateRatio);
    let accum = 0;
    let count = 0;

    for (let i = offsetBuffer; i < nextOffsetBuffer && i < buffer.length; i++) {
      accum += buffer[i];
      count++;
    }

    result[offsetResult] = count > 0 ? accum / count : 0;
    offsetResult++;
    offsetBuffer = nextOffsetBuffer;
  }

  return result;
}

function floatTo16BitPCM(samples) {
  const buffer = new ArrayBuffer(samples.length * 2);
  const view = new DataView(buffer);
  let offset = 0;

  for (let i = 0; i < samples.length; i++, offset += 2) {
    const clamped = Math.max(-1, Math.min(1, samples[i]));
    view.setInt16(
      offset,
      clamped < 0 ? clamped * 0x8000 : clamped * 0x7fff,
      true
    );
  }

  return buffer;
}

function arrayBufferToBase64(buffer) {
  const bytes = new Uint8Array(buffer);
  let binary = "";
  const chunkSize = 0x8000;

  for (let i = 0; i < bytes.length; i += chunkSize) {
    const chunk = bytes.subarray(i, i + chunkSize);
    binary += String.fromCharCode.apply(null, chunk);
  }

  return btoa(binary);
}

export async function startRecording(
  dotNetRef,
  sampleRate = DEFAULT_SAMPLE_RATE
) {
  if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
    notifyError("このブラウザーはマイク入力に対応していません。");
    return;
  }

  try {
    desiredSampleRate = sampleRate;
    dotNetCallback = dotNetRef;

    console.debug("[voiceRecorder] Requesting microphone stream...");

    mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
    audioContext = new (window.AudioContext || window.webkitAudioContext)({
      sampleRate: desiredSampleRate,
    });

    console.debug("[voiceRecorder] Microphone stream acquired", {
      desiredSampleRate,
      audioContextSampleRate: audioContext.sampleRate,
    });

    mediaStreamSource = audioContext.createMediaStreamSource(mediaStream);
    processor = audioContext.createScriptProcessor(4096, 1, 1);

    processor.onaudioprocess = (event) => {
      const channelData = event.inputBuffer.getChannelData(0);
      recordingBuffers.push(new Float32Array(channelData));
    };

    mediaStreamSource.connect(processor);
    processor.connect(audioContext.destination);

    recording = true;
    console.debug("[voiceRecorder] Recording started");
    notifyStatus("recording");
  } catch (error) {
    stopRecording();
    notifyError(error?.message ?? "マイクの初期化に失敗しました。");
    cleanup();
  }
}

export function stopRecording() {
  if (!recording) {
    console.debug(
      "[voiceRecorder] stopRecording called but not actively recording"
    );
    return;
  }

  recording = false;
  const currentSampleRate = audioContext
    ? audioContext.sampleRate
    : desiredSampleRate;

  console.debug("[voiceRecorder] Stopping recording", {
    currentSampleRate,
    desiredSampleRate,
  });

  if (processor) {
    processor.disconnect();
  }

  if (mediaStreamSource) {
    mediaStreamSource.disconnect();
  }

  if (mediaStream) {
    mediaStream.getTracks().forEach((track) => track.stop());
  }

  const downsampleSourceRate = currentSampleRate;

  const merged = mergeBuffers(recordingBuffers);
  const downsampled = downsampleBuffer(
    merged,
    downsampleSourceRate,
    desiredSampleRate
  );
  const pcmBuffer = floatTo16BitPCM(downsampled);
  const base64 = arrayBufferToBase64(pcmBuffer);

  console.debug("[voiceRecorder] Recording complete", {
    mergedLength: merged.length,
    downsampledLength: downsampled.length,
    pcmBytes: pcmBuffer.byteLength,
    base64Length: base64.length,
  });

  notifyStatus("stopped");
  invokeDotNet("OnVoiceRecorderComplete", base64, desiredSampleRate);

  cleanup();
}

function cleanup() {
  if (processor) {
    processor.disconnect();
    processor = null;
  }

  if (mediaStreamSource) {
    mediaStreamSource.disconnect();
    mediaStreamSource = null;
  }

  if (mediaStream) {
    mediaStream.getTracks().forEach((track) => track.stop());
    mediaStream = null;
  }

  if (audioContext) {
    if (audioContext.state !== "closed") {
      audioContext.close().catch(() => {
        /* noop */
      });
    }
    audioContext = null;
  }

  resetRecordingState();
}

export function dispose() {
  cleanup();
  dotNetCallback = null;
}
