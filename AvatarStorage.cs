using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarStore", menuName = "TexasHoldem/AvatarStorage", order = 1)]
public class AvatarStorage: ScriptableObject
{
    public List<Sprite> list_avatar_sprites;
    public Sprite private_avatar_sprite;
    public Sprite lobby_empty_avatar_sprite;
    public Sprite room_empty_avatar_sprite;
}
