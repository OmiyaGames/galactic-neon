using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour
{
    private static BackgroundManager msInstance = null;

    public enum Type
    {
        Shop = -1,
        Default = 0,
    }

    [System.Serializable]
    public struct BackgroundSet
    {
        public Type type;
        public BackgroundGenerator generator;
        public Texture background;
        public AudioClip music;
    }

    public BackgroundSet[] allSets;
    [Header("Updated Components")]
    public Camera mainCamera;
    public Renderer backgroundMesh;
    public string mainTexture = "Texture 1";
    public string blendTexture = "Texture 2";
    public string blendFloat = "_Blend";
    public AudioSource mainBackgroundSoundSource;
    public AudioSource fadeInSoundSource;
    [Header("Background Color")]
    public float hueChangeSpeed = 0.1f;
    public float saturation = 1f;
    public float brightness = 0.1f;
    [Header("Background Movement")]
    public Vector2 shiftBackgroundBy = new Vector2(0.01f, 0.01f);
    [Header("Transition")]
    public float transitionLength = 1f;
    public float transitionTextureSpeed = 1f;
    public float transitionMusicSpeed = 1f;

    float hue = 0;
    Color backgroundColor = Color.red;
    Type currentType = Type.Shop;
    float timeToTransition = -1f;
    readonly Dictionary<Type, BackgroundSet> backgroundDictionary = new Dictionary<Type, BackgroundSet>();
    Material backgroundMaterial;
    Transform backgroundMeshTransform;
    Vector3 meshPosition, diffPosition;
    Vector2 backgroundOffset = Vector2.zero;
    float textureTransition = 0f;
    float volumeTransition = 1f;
    float maxVolume = 1f;
    BackgroundSet currentBackgroundSet;

    AudioSource soundSource1;
    AudioSource soundSource2;
    bool playingSource1 = true;

    #region Properties

    public static BackgroundManager Instance
    {
        get
        {
            return msInstance;
        }
    }

    public Type DisplayedBackground
    {
        get
        {
            return currentType;
        }
        set
        {
            if (currentType != value)
            {
                // Setup flag
                currentType = value;
                timeToTransition = Time.time;
                StartTransition();
            }
        }
    }

    public void ApplyExplosionForce(Vector3 position)
    {
        currentBackgroundSet.generator.ApplyExplosionForce(position);
    }
    
    #endregion

    // Use this for initialization
    void Awake()
    {
        msInstance = this;

        // Update hue
        hue = Random.value;
        IPowerUp.FromHSV(hue, saturation, brightness, ref backgroundColor);
        mainCamera.backgroundColor = backgroundColor;

        // Fill the dictionary
        currentBackgroundSet = allSets[0];
        backgroundDictionary.Clear();
        for(int index = 0; index < allSets.Length; ++index)
        {
            backgroundDictionary.Add(allSets[index].type, allSets[index]);
        }

        if(backgroundMesh != null)
        {
            // Retrieving the background material
            backgroundMaterial = backgroundMesh.material;
            backgroundMeshTransform = backgroundMesh.transform;
            meshPosition = backgroundMeshTransform.position;

            // Grab texture transition
            textureTransition = backgroundMaterial.GetFloat(blendFloat);
            backgroundMaterial.SetTexture(mainTexture, currentBackgroundSet.background);
        }

        // Grab volume transition
        maxVolume = mainBackgroundSoundSource.volume;
        volumeTransition = maxVolume;
        soundSource1 = mainBackgroundSoundSource;
        soundSource2 = fadeInSoundSource;
        playingSource1 = true;

        // Startup current settings
        currentBackgroundSet.generator.Active = true;
        mainBackgroundSoundSource.clip = currentBackgroundSet.music;
    }

    void LateUpdate()
    {
        // Update sprites
        foreach(BackgroundSprite sprite in BackgroundSprite.AllBackgroundSprites)
        {
            if(sprite.ThisObject.activeSelf == true)
            {
                if((sprite.StartTime > 0) && ((Time.time - sprite.StartTime) > sprite.duration))
                {
                    if(sprite.DeactivateNextLoop == false)
                    {
                        sprite.Start();
                    }
                    else
                    {
                        sprite.ThisObject.SetActive(false);
                    }
                }
                else if(sprite.RotateSpeed.CompareTo(0) > 0)
                {
                    sprite.SpriteTransform.Rotate(0, 0, (sprite.RotateSpeed * Time.deltaTime));
                }
            }
        }

        // Change background color
        UpdateBackgroundColor();

        // Change the background offset
        if(backgroundMesh != null)
        {
            OffsetBackgroundTexture();
        }

        // Check if we need to transition to the next background
        if(timeToTransition.CompareTo(0) >= 0)
        {
            UpdateTransition();

            // Check the transition length
            if((Time.time - timeToTransition) > transitionLength)
            {
                EndTransition();
                timeToTransition = -1f;
            }
        }
    }
    
    // Update is called once per frame
    void OnDestroy()
    {
        msInstance = null;
    }

    #region Update Helper Methods

    void UpdateBackgroundColor()
    {
        hue += hueChangeSpeed * Time.deltaTime;
        if (hue > 1)
        {
            hue = 0;
        }
        IPowerUp.FromHSV(hue, saturation, brightness, ref backgroundColor);
        if(backgroundMesh != null)
        {
            backgroundMaterial.color = backgroundColor;
        }
    }

    void OffsetBackgroundTexture()
    {
        // Calculate offset
        diffPosition = backgroundMeshTransform.position - meshPosition;
        backgroundOffset.x += diffPosition.x * shiftBackgroundBy.x;
        backgroundOffset.y += diffPosition.y * shiftBackgroundBy.y;

        // Normalize texture coordinates
        while(backgroundOffset.x < 0)
        {
            backgroundOffset.x += 1;
        }
        while(backgroundOffset.x > 1)
        {
            backgroundOffset.x -= 1;
        }
        while(backgroundOffset.y < 0)
        {
            backgroundOffset.y += 1;
        }
        while(backgroundOffset.y > 1)
        {
            backgroundOffset.y -= 1;
        }

        // Set the offset
        backgroundMaterial.SetTextureOffset(mainTexture, backgroundOffset);
        backgroundMaterial.SetTextureOffset(blendTexture, backgroundOffset);

        // Update the mesh position
        meshPosition = backgroundMeshTransform.position;
    }

    #endregion

    #region Transition Helper Methods

    void StartTransition()
    {
        // Grab the current background
        currentBackgroundSet.generator.Active = false;
        currentBackgroundSet = backgroundDictionary[currentType];
        currentBackgroundSet.generator.Active = true;

        // Switch textures
        Texture lastTexture = backgroundMaterial.GetTexture(mainTexture);
        if(lastTexture.Equals(currentBackgroundSet.background) == false)
        {
            backgroundMaterial.SetTexture(blendTexture, lastTexture);
            backgroundMaterial.SetTexture(mainTexture, currentBackgroundSet.background);

            // Switch blending factor
            textureTransition = 1f - textureTransition;
            backgroundMaterial.SetFloat(blendFloat, textureTransition);
        }

        // Switch the audio clips
        if(mainBackgroundSoundSource.clip.Equals(currentBackgroundSet.music) == false)
        {
            // Grab the original volume
            volumeTransition = (maxVolume - mainBackgroundSoundSource.volume);

            // Swap the sound source
            playingSource1 = !playingSource1;
            if(playingSource1 == true)
            {
                mainBackgroundSoundSource = soundSource1;
                fadeInSoundSource = soundSource2;
            }
            else
            {
                mainBackgroundSoundSource = soundSource1;
                fadeInSoundSource = soundSource2;
            }

            // Update the main sound source, and play it.
            mainBackgroundSoundSource.clip = currentBackgroundSet.music;
            mainBackgroundSoundSource.volume = volumeTransition;
            mainBackgroundSoundSource.Play();
        }
    }

    void UpdateTransition()
    {
        // Transition background blend factor
        textureTransition = Mathf.MoveTowards(textureTransition, 0, (Time.deltaTime * transitionTextureSpeed));
        backgroundMaterial.SetFloat(blendFloat, textureTransition);

        // Transition the sound effect
        volumeTransition = Mathf.MoveTowards(volumeTransition, maxVolume, (Time.deltaTime * transitionMusicSpeed));
        mainBackgroundSoundSource.volume = volumeTransition;
        fadeInSoundSource.volume = (maxVolume - volumeTransition);
    }

    void EndTransition()
    {
        fadeInSoundSource.Stop();
    }

    #endregion
}
