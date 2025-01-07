using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GranularSynth : MonoBehaviour
{

    public AudioClip[] clips;
    public bool testPlay;

    int[] sampleLengths;
    float[][] samples;


    private double sampleRate = 0.0F;
    private bool running = false;


    List<Grain> grains;
    void OnEnable()
    {

        samples = new float[clips.Length][];
        sampleLengths = new int[clips.Length];

        for (int i = 0; i < clips.Length; i++)
        {
            sampleLengths[i] = clips[i].samples;
            samples[i] = new float[clips[i].samples * clips[i].channels];
            clips[i].GetData(samples[i], 0);

        }

        GetComponent<AudioSource>().Play();

        sampleRate = AudioSettings.outputSampleRate;

        running = true;
        positionInGrain = 0;

        grains = new List<Grain>();
        tmpGrains = new List<Grain>();


        lastGrainTime = Time.time;

    }



    // Start is called before the first frame update
    void Start()
    {

    }



    public float lastGrainTime;

    public float timeBetweenGrains = 1;

    public int numGrains;

    public int maxGrains = 30;

    public bool newGrains = false;
    // Update is called once per frame
    void Update()
    {

        if (grains != null)
        {
            numGrains = grains.Count;


            if (testPlay)
            {
                if (Time.time - lastGrainTime > timeBetweenGrains)
                {
                    lastGrainTime = Time.time;
                    NewGrain();
                }
            }


            lock (grains)

                for (int i = 0; i < tmpGrains.Count; i++)
                {
                    for (int j = 0; j < grains.Count; j++)
                    {
                        if (tmpGrains[i].id == grains[j].id)
                        {
                            grains[j] = tmpGrains[i];
                        }
                    }
                }

            newGrains = true;


            for (int i = grains.Count - 1; i >= 0; i--)
            {
                if (grains[i].active == 0)
                {
                    //                    print("REMOVING");
                    grains.RemoveAt(i);
                }
            }


            tmpGrains = new List<Grain>(grains);
        }

    }





    public struct Grain
    {
        public int startPositionInSample;
        public float currentPositionInSample;

        public int fPositionInSample;
        public float playbackSpeed;
        public float loudness;

        public int totalNumberOfSamples;
        public int active;
        public int id;

        public int clipID;
        public float leftRightBlend;


        Grain(int startPositionInSample, int currentPositionInSample, int fPositionInSample, float playbackSpeed, int totalNumberOfSamples, float loudness, float leftRightBlend)
        {
            this.startPositionInSample = startPositionInSample;
            this.currentPositionInSample = 0;
            this.fPositionInSample = fPositionInSample;
            this.playbackSpeed = playbackSpeed;
            this.totalNumberOfSamples = totalNumberOfSamples;
            this.active = 1;
            this.id = Random.Range(0, 100000000);
            this.loudness = loudness;
            this.clipID = 0;
            this.leftRightBlend = leftRightBlend;
        }


    }


    public void NewGrain()
    {
        if (grains.Count >= maxGrains)
        {
            return;
        }

        Grain g = new Grain();
        g.clipID = 0;
        g.startPositionInSample = 0;
        g.currentPositionInSample = 0;
        g.playbackSpeed = 1; ;//.Range(.4f, 2.1f);
        g.totalNumberOfSamples = sampleLengths[g.clipID] / 2;
        g.active = 1;
        g.loudness = 1;
        g.id = Random.Range(0, 100000000);
        g.leftRightBlend = .5f;
        grains.Add(g);

    }

    public void NewGrain(float length, float speed, float loudness, int clipID)
    {

        if (grains.Count >= maxGrains)
        {
            return;
        }
        Grain g = new Grain();
        g.clipID = clipID;
        g.startPositionInSample = Random.Range(0, sampleLengths[g.clipID]);
        g.currentPositionInSample = 0;
        g.playbackSpeed = speed;
        g.totalNumberOfSamples = (int)((float)sampleLengths[g.clipID] * length);
        g.active = 1;
        g.loudness = loudness;
        g.id = Random.Range(0, 100000000);
        g.leftRightBlend = .5f;
        grains.Add(g);


    }


    public void NewGrain(float length, float speed, float loudness, float positionInSample, int clipID)
    {

        if (grains.Count >= maxGrains)
        {
            return;
        }


        Grain g = new Grain();
        g.clipID = clipID;
        g.totalNumberOfSamples = (int)((float)sampleRate * length);
        g.startPositionInSample = (int)(positionInSample * (float)(sampleLengths[g.clipID] - g.totalNumberOfSamples));
        g.currentPositionInSample = 0;
        g.playbackSpeed = speed;
        g.active = 1;
        g.loudness = loudness;
        g.id = Random.Range(0, 100000000);
        g.leftRightBlend = .5f;
        grains.Add(g);

    }



    public void NewGrain(float length, float speed, float loudness, float positionInSample, int clipID, float leftRightBlend)
    {

        if (grains.Count >= maxGrains)
        {
            return;
        }


        Grain g = new Grain();
        g.clipID = clipID;
        g.totalNumberOfSamples = (int)((float)sampleRate * length);
        g.startPositionInSample = (int)(positionInSample * (float)(sampleLengths[g.clipID] - g.totalNumberOfSamples));
        g.currentPositionInSample = 0;
        g.playbackSpeed = speed;
        g.active = 1;
        g.loudness = loudness;
        g.id = Random.Range(0, 100000000);
        g.leftRightBlend = leftRightBlend;
        grains.Add(g);

    }



    int positionInGrain;
    List<Grain> tmpGrains;


    float sample1;
    float sample2;
    float fSample;
    int fPositionInSampleCeil;
    int fPositionInSampleFloor;

    float fPositionInSample;
    float nInSample;
    float env;
    float lerpVal;
    Grain g;

    public double sample;
    public int dataLen;
    void OnAudioFilterRead(float[] data, int channels)
    {

        lock (tmpGrains)
        {
            lock (grains)
            {

                // copy in our new grains
                if (newGrains == true)
                {
                    tmpGrains = new List<Grain>(grains);
                    newGrains = false;
                }

                if (!running)
                    return;

                sample = AudioSettings.dspTime * sampleRate;
                dataLen = data.Length / channels;

                int n = 0;
                while (n < dataLen)
                {
                    for (int i = 0; i < tmpGrains.Count; i++)
                    {

                        g = tmpGrains[i];

                        fPositionInSample = ((float)g.startPositionInSample + g.currentPositionInSample) % ((float)sampleLengths[g.clipID] / 2);
                        nInSample = ((float)g.currentPositionInSample * 2) / (float)g.totalNumberOfSamples;


                        // quick in long fadeOut
                        env = Mathf.Min(nInSample * 10, Mathf.Pow(1 - nInSample, 2));
                        env *= g.loudness;


                        //  if (i == 0) { print(nInSample); }
                        //env = Mathf.Clamp(1 - Mathf.Abs(nInSample - .5f) * 2, 0, g.loudness); ;


                        fPositionInSampleCeil = (int)Mathf.Ceil(fPositionInSample);
                        fPositionInSampleFloor = (int)Mathf.Ceil(fPositionInSample);

                        if (fPositionInSampleCeil * 2 + 1 >= sampleLengths[g.clipID])
                        {
                            fPositionInSampleCeil -= sampleLengths[g.clipID] / 2;
                            fPositionInSampleFloor -= sampleLengths[g.clipID] / 2;
                        }



                        lerpVal = fPositionInSample - (float)fPositionInSampleFloor;

                        sample1 = samples[g.clipID][2 * fPositionInSampleFloor];
                        sample2 = samples[g.clipID][2 * fPositionInSampleCeil];
                        fSample = sample1 + (sample2 - sample1) * lerpVal;

                        data[n * 2] += fSample * env * (1 - (g.leftRightBlend)) * 2;


                        sample1 = samples[g.clipID][2 * fPositionInSampleFloor + 1];
                        sample2 = samples[g.clipID][2 * fPositionInSampleCeil + 1];
                        fSample = sample1 + (sample2 - sample1) * lerpVal;

                        data[n * 2 + 1] += fSample * env * (g.leftRightBlend * 2);

                        g.currentPositionInSample += g.playbackSpeed;

                        if (g.currentPositionInSample >= g.totalNumberOfSamples / 2)
                        {
                            g.active = 0;
                        }

                        tmpGrains[i] = g;



                    }



                    data[n * 2] = Mathf.Clamp(data[n * 2], -1, 1);
                    data[n * 2 + 1] = Mathf.Clamp(data[n * 2 + 1], -1, 1);

                    n++;

                }
            }
        }

    }

}