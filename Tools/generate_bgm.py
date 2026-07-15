#!/usr/bin/env python3
"""Generate seamless, original instrumental loops for Monster Emotion Clinic."""

from pathlib import Path
import wave
import numpy as np

SR = 22050
OUT = Path(__file__).resolve().parents[1] / "Assets/Resources/Audio/Music"


def hz(note):
    return 440.0 * 2 ** ((note - 69) / 12)


def add_tone(buf, start, dur, note, amp, kind="bell", pan=0.5):
    i0, i1 = int(start * SR), min(len(buf), int((start + dur) * SR))
    if i1 <= i0:
        return
    t = np.arange(i1 - i0) / SR
    f = hz(note)
    if kind == "pad":
        x = (np.sin(2*np.pi*f*t) + .32*np.sin(2*np.pi*f*2*t) + .12*np.sin(2*np.pi*f*3*t)) / 1.44
        env = np.sin(np.pi * np.minimum(t / max(dur, .01), 1)) ** .55
    elif kind == "piano":
        x = np.sin(2*np.pi*f*t) + .24*np.sin(2*np.pi*f*2*t) + .1*np.sin(2*np.pi*f*3*t)
        env = (1-np.exp(-t*35)) * np.exp(-t*2.5)
    elif kind == "pluck":
        x = np.sin(2*np.pi*f*t) + .3*np.sin(2*np.pi*f*2*t)
        env = (1-np.exp(-t*60)) * np.exp(-t*5.2)
    else:
        x = np.sin(2*np.pi*f*t) + .45*np.sin(2*np.pi*f*2.01*t) + .15*np.sin(2*np.pi*f*3.99*t)
        env = (1-np.exp(-t*45)) * np.exp(-t*3.2)
    sig = amp * x * env
    buf[i0:i1, 0] += sig * np.cos(pan*np.pi/2)
    buf[i0:i1, 1] += sig * np.sin(pan*np.pi/2)


def render(name, bpm, bars, progression, melody, style):
    beat = 60 / bpm
    bar = beat * 4
    duration = bars * bar
    buf = np.zeros((int(duration * SR), 2), np.float64)
    for b in range(bars):
        chord = progression[b % len(progression)]
        for n in chord:
            add_tone(buf, b*bar, bar, n, .037 if style != "puzzle" else .03, "pad", .28 + .44*((n%5)/4))
        root = min(chord) - 12
        add_tone(buf, b*bar, beat*2.8, root, .03, "piano", .42)
        if style == "puzzle":
            for k in range(8):
                n = chord[(k + b) % len(chord)] + (12 if k in (3, 7) else 0)
                add_tone(buf, b*bar+k*beat/2, beat*.42, n, .026, "pluck", .25+.5*(k%2))
    phrase_bars = len(melody)
    for b in range(bars):
        events = melody[b % phrase_bars]
        for pos, length, note in events:
            add_tone(buf, b*bar+pos*beat, length*beat, note, .04, "bell" if style != "ward" else "piano", .34+.32*((b+pos)%2))
    # Low, filtered-feeling room breath; deterministic and loop-safe fade edges.
    t = np.arange(len(buf))/SR
    breath = .004*np.sin(2*np.pi*.125*t) * np.sin(2*np.pi*55*t)
    buf[:, 0] += breath
    buf[:, 1] += np.roll(breath, 97)
    edge = min(int(.08*SR), len(buf)//2)
    buf[:edge] *= np.linspace(0, 1, edge)[:, None]
    buf[-edge:] *= np.linspace(1, 0, edge)[:, None]
    peak = np.max(np.abs(buf))
    buf = np.tanh(buf / max(peak, .01) * .72) * .78
    pcm = (buf * 32767).astype('<i2')
    OUT.mkdir(parents=True, exist_ok=True)
    with wave.open(str(OUT / f"{name}.wav"), "wb") as f:
        f.setnchannels(2); f.setsampwidth(2); f.setframerate(SR); f.writeframes(pcm.tobytes())
    print(f"{name}: {duration:.1f}s")


def main():
    # All four cues share the rising 1-2-5 motif: WuWu's feeling of being noticed.
    render("BGM_Login_MidnightClinic", 72, 12,
           [(48,55,60,64),(45,52,57,60),(41,48,53,57),(43,50,55,59)],
           [[(0,1,72),(1.5,.7,74),(2.5,1.2,79)],[],[(1,.8,67),(2,.8,69),(3,1,72)],[]], "login")
    render("BGM_Main_GentleReception", 84, 16,
           [(48,55,60,64),(50,57,62,65),(45,52,57,60),(43,50,55,59)],
           [[(0,.7,72),(1,.7,74),(2,1.4,79)],[(1,.7,76),(2,.7,74),(3,1,72)],[],[(0,1,67),(2,1,71)]], "main")
    render("BGM_Ward_QuietCompanionship", 66, 12,
           [(45,52,57,60),(41,48,53,57),(48,55,60,64),(43,50,55,59)],
           [[(0,1.2,69),(2,1.5,72)],[],[(.5,.8,67),(2,.8,69),(3,1,74)],[]], "ward")
    render("BGM_Puzzle_MistAndName", 96, 16,
           [(45,52,57,60),(48,55,60,64),(50,57,62,65),(43,50,55,59)],
           [[(0,.5,69),(1,.5,71),(2,1,76)],[],[(0,.5,72),(1,.5,74),(2,1,79)],[]], "puzzle")


if __name__ == "__main__":
    main()
