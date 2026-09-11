"""Original 120 BPM synthwave loop, synthesized without samples or third-party music."""
from pathlib import Path
from array import array
import math, random, wave
root=Path(__file__).resolve().parents[1]
rate=22050; duration=64; size=rate*duration; mix=array('f',[0])*size
rng=random.Random(731)
def tone(start,length,midi,volume,pluck=False):
    frequency=440*2**((midi-69)/12)
    for i in range(int(length*rate)):
        t=i/rate; fade=min(1,t/.025)*min(1,(length-t)/.12)
        envelope=math.exp(-t*5) if pluck else fade
        value=math.sin(2*math.pi*frequency*t)+.24*math.sin(4*math.pi*frequency*t)+.1*math.sin(6*math.pi*frequency*t)
        n=int(start*rate)+i
        if n<size:mix[n]+=value*volume*envelope
for bar in range(32):
    root_note=[45,41,48,43][bar//2%4]
    for note in [root_note+12,root_note+15,root_note+19]:tone(bar*2,2,note,.034)
    for beat in range(4):
        start=bar*2+beat*.5;tone(start,.43,root_note-12,.16)
        for i in range(int(.22*rate)):
            t=i/rate;mix[int(start*rate)+i]+=.34*math.sin(2*math.pi*(48*t+2.1*(1-math.exp(-t*32))))*math.exp(-t*17)
        if beat%2:
            for i in range(int(.16*rate)):
                t=i/rate;mix[int(start*rate)+i]+=(rng.uniform(-1,1)*.14+math.sin(2*math.pi*185*t)*.05)*math.exp(-t*27)
    for step in range(8):
        start=bar*2+step*.25;tone(start,.3,root_note+24+[0,7,3,12,7,15,12,7][step],.075,True)
        for i in range(int(.045*rate)):
            t=i/rate;mix[int(start*rate)+i]+=rng.uniform(-1,1)*.045*math.exp(-t*90)
pcm=array('h')
for n,value in enumerate(mix):
    fade=min(1,n/rate*.8,(size-n)/rate*.8)
    delay=mix[n-int(rate*.375)]*.19 if n>=int(rate*.375) else 0
    pcm.extend((int(math.tanh((value+delay)*fade)*26000),int(math.tanh((value-delay*.25)*fade)*26000)))
output=root/'Assets/Resources/Audio/CoastalDrive.wav';output.parent.mkdir(parents=True,exist_ok=True)
with wave.open(str(output),'wb') as f:f.setnchannels(2);f.setsampwidth(2);f.setframerate(rate);f.writeframes(pcm.tobytes())
print('Original soundtrack:',output, duration,'seconds')
