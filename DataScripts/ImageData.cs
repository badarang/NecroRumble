using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

[CreateAssetMenu(fileName = "ImageData", menuName = "YourMenuPath/ImageData", order = 1)]
public class ImageData : ScriptableObject
{
    [SerializeField] private List<SpritePair> _imagePairList = new List<SpritePair>();

    private Dictionary<SituationType, SpritePair> _imagePairDictionary;

    public Dictionary<SituationType, SpritePair> ImagePairDictionary
    {
        get
        {
            if (_imagePairDictionary == null)
                CreateDictionary();
            return _imagePairDictionary;
        }
    }

    [System.Serializable]
    public class SpritePair
    {
        [SerializeField] private SituationType _identifier; // Use an enum as the identifier
        [SerializeField] private Sprite _keyboardSprite;
        [SerializeField] private Sprite _gamepadSprite;

        public SituationType Identifier => _identifier;
        public Sprite KeyboardSprite => _keyboardSprite;
        public Sprite GamepadSprite => _gamepadSprite;
    }

    public enum SituationType
    {
        BoneSpearAttack,
        CorpseRevive,
        Grind,
    }

    private void CreateDictionary()
    {
        _imagePairDictionary = new Dictionary<SituationType, SpritePair>();

        foreach (var pair in _imagePairList)
        {
            if (!_imagePairDictionary.ContainsKey(pair.Identifier))
            {
                _imagePairDictionary.Add(pair.Identifier, pair);
            }
            else
            {
                Debug.LogWarning($"Duplicate identifier found: {pair.Identifier}. Ignoring the second entry.");
            }
        }
    }

    // public Sprite GetSprite(SpriteType spriteType)
    // {
    //     if (ManagerRoot.Input.LastUsedInputType == DeviceChangeListener.ControlDeviceType.KeyboardAndMouse)
    //     {
    //         return ImagePairDictionary[spriteType].KeyboardSprite;
    //     }
    //     else
    //     {
    //         return ImagePairDictionary[spriteType].GamepadSprite;
    //     }
    // }
}