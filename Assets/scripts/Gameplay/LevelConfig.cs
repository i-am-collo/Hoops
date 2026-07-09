using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "HOOPS/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public int levelNumber;
    public float hoopSpeed;
    public Sprite backgroundSprite;
    public float difficultyValue;
}
