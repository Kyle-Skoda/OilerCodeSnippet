using UnityEngine;

public abstract class AUseAbility : MonoBehaviour
{
    [HideInInspector]
    public Weapon weapon;
    [HideInInspector]
    public bool Active = false;

    protected CardAbility card;
    protected int cost;
    protected int uses;

    private void Awake() => GetComponent<AbilityUser>().ability = this;
    public int GetUses() => uses;
    public void SetWeapon(Weapon weapon) => this.weapon = weapon;

    public void SetCard(Card card)
    {
        //Add card logic to the gameobject for unique logic between cards.
        System.Type MyScriptType = System.Type.GetType(card.cardAbility + ",Assembly-CSharp");
        gameObject.AddComponent(MyScriptType);
        this.card = GetComponent<CardAbility>();
        this.card.card = card;
        cost = card.cost;
        uses = card.uses;
    }

    public abstract void UseAbility();
}
