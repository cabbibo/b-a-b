using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

[ExecuteAlways]
public class TimelinePlayback : MonoBehaviour
{


    public PlayableDirector director;

    public float push; // nudge offset

    public float time;
    public float rawTime;




    public int whichTrack;

    public IEnumerable<TimelineClip> trackClips;
    public IEnumerable<IMarker> markers;


    public bool looping;




    // public IEnumerable<TimelineMarkers> trackClips;

    // Start is called before the first frame update
    void OnEnable()
    {
        GetClipInfo();
    }

    void GetClipInfo()
    {
        // Check if the director and its playableAsset are assigned
        if (director && director.playableAsset)
        {
            TimelineAsset timeline = director.playableAsset as TimelineAsset;

            print(timeline);
            print(timeline.outputs);

            var track = timeline.GetOutputTrack(1);

            trackClips = track.GetClips();

            print(trackClips.Count());
            print("NUM TRACKS ^^^^");

            foreach (var clip in track.GetClips())
            {
                Debug.Log($"Clip Name: {clip.displayName}, Start: {clip.start}, Duration: {clip.duration}");
            }

            MarkerTrack mt = timeline.markerTrack;


            var tracks = mt.GetMarkers();

            markers = tracks;

            foreach (var marker in markers)
            {
                Debug.Log("HI");
                print(marker.time);
            }

        }
    }

    // Update is called once per frame
    void Update()
    {


        float fTime = time;


        time = (float)director.time;

        time += push / 60;

        int id = 0;
        foreach (TimelineClip clip in trackClips)
        {

            if (time >= clip.start && time < clip.end)
            {
                whichTrack = id;
                time = (time - (float)clip.start) / ((float)clip.end - (float)clip.start);
                rawTime = time -(float)clip.start;

                break;
            }
            id++;
        }

        if (time > 1)
        {
            time = 1;
        }
        else if (time < 0)
        {
            time = 0;
        }


     /*   numStems = numStemss[whichTrack];
        fullFFTTexture = fullFFTTextures[whichTrack];
        fullWaveformTexture = fullWaveformTextures[whichTrack];
        fullPowerTexture = fullPowerTextures[whichTrack];
        colorMap = colorMaps[whichTrack];



        Shader.SetGlobalTexture("_AudioMap", fullFFTTexture);
        Shader.SetGlobalTexture("_WaveformMap", fullWaveformTexture);
        Shader.SetGlobalTexture("_PowerMap", fullPowerTexture);

        Shader.SetGlobalTexture("_ColorMap", colorMap);
        Shader.SetGlobalInt("_WhichTrack", whichTrack);
        Shader.SetGlobalFloat("_NumStems", numStems);
        Shader.SetGlobalFloat("_Timeline", time);
        Shader.SetGlobalFloat("_AudioMultiplierExtra", _AudioMultiplierExtra);
        Shader.SetGlobalFloat("_AudioSizeMultiplier", _AudioSizeMultiplier);
*/


    }






    public void LoopToFirst()
    {
        print("helllo");

        //print(director.time);

        float loopEndTime = (float)director.time;


        foreach (var marker in markers)
        {

            print("MARKER");
            print(marker.time);
            print(director.time);
            print(director.time - marker.time);

            if (Mathf.Abs((float)director.time - (float)marker.time) < .3f)
            {
                loopEndTime = (float)marker.time;
            }
        }

        float closestTime = 10000;
        float otherMarkerTime = 0;
        foreach (var marker in markers)
        {

            float v = (float)loopEndTime - (float)marker.time;

            if (v < 0)
            {
                continue;
            }
            else if (v > 0)
            {
                otherMarkerTime = (float)marker.time;
            }
            else if (v == 0)
            {
                print("SAME");
            }


        }



        if (looping)
        {

            print(otherMarkerTime);
            director.time = otherMarkerTime;

        }

    }
/*

    public float SampleAudioPower(int id)
    {

        // time is normalized 

        float x = ((float)id -.05f) / ((float)numStems + 1);

        if( id >= numStems ){
            return 0;
        }
        float y = time;

        //float v = 1;
        float v = fullPowerTexture.GetPixelBilinear(x, y).r;

        return v;



    }


    public void SetComputeData(ComputeShader shader, int kernel)
    {

        //  print("SET COMPUTE DATA");
        shader.SetTexture(kernel, "_AudioMap", fullFFTTexture);
        shader.SetTexture(kernel, "_WaveformMap", fullWaveformTexture);
        shader.SetTexture(kernel, "_PowerMap", fullPowerTexture);
        shader.SetFloat("_NumStems", numStems);
        shader.SetFloat("_Timeline", time);
        shader.SetInt("_WhichTrack", whichTrack);
        shader.SetFloat("_AudioMultiplierExtra", _AudioMultiplierExtra);
        shader.SetFloat("_AudioSizeMultiplier", _AudioSizeMultiplier);

    }

    public void SetMPBData(MaterialPropertyBlock mpb)
    {
        mpb.SetTexture("_AudioMap", fullFFTTexture);
        mpb.SetTexture("_WaveformMap", fullWaveformTexture);
        mpb.SetTexture("_PowerMap", fullPowerTexture);
        mpb.SetInt("_WhichTrack", whichTrack);
        mpb.SetFloat("_NumStems", numStems);
        mpb.SetFloat("_Timeline", time);
        mpb.SetFloat("_AudioMultiplierExtra", _AudioMultiplierExtra);
        mpb.SetFloat("_AudioSizeMultiplier", _AudioSizeMultiplier);
    }

*/

}