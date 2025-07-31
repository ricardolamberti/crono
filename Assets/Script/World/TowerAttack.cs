using System.Collections;
using UnityEngine;
using GameConstants;

public class TowerAttack : MonoBehaviour
{
    private Vector2Int gridPos;
    private string owner;
    private int range;
    private int damage;
    private WeaponType weapon;
    private float timer = 0f;
    private float interval = 1f;

    public void Initialize(Vector2Int pos)
    {
        gridPos = pos;
        if (MapState.cellMap.TryGetValue(pos, out var cell))
            owner = cell.owner;
        if (MapState.buildings.TryGetValue(pos, out var building))
        {
            range = building.AttackRange;
            damage = building.RangedDamage;
            weapon = building.RangedWeapon;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            var target = FindTarget();
            if (target != null)
                StartCoroutine(Attack(target));
        }
    }

    Character FindTarget()
    {
        foreach (var c in GameObject.FindObjectsOfType<Character>())
        {
            if (c.owner == owner) continue;
            if (Vector2Int.Distance(c.GetGridPosition(), gridPos) <= range)
                return c;
        }
        return null;
    }

    IEnumerator Attack(Character target)
    {
        if (target == null) yield break;
        if (weapon != WeaponType.None)
        {
            yield return ShootProjectile(target.transform.position);
        }
        if (target.TryGetComponent(out HealthComponent hc))
            hc.TakeDamage(damage);
    }

    IEnumerator ShootProjectile(Vector3 targetPos)
    {
        if (MapLoader.instance == null) yield break;
        Sprite s = MapLoader.instance.GetWeaponSprite(weapon);
        if (s == null) yield break;
        GameObject proj = new GameObject("Projectile");
        var sr = proj.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.sortingOrder = 12;
        proj.transform.position = transform.position + new Vector3(0f, 0.6f, 0f);
        Vector3 start = proj.transform.position;
        Vector3 end = targetPos + new Vector3(0f, 0.6f, 0f);
        float t = 0f;
        while (t < 1f)
        {
            proj.transform.position = Vector3.Lerp(start, end, t);
            t += Time.deltaTime * 5f;
            yield return null;
        }
        Destroy(proj);
    }
}
