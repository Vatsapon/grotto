using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BossOne_Manager : MonoBehaviour
{
    public float MaxHealth = 3;
    public Vector3 CenterPos = Vector3.zero;
    public GameObject Projectile;
    public float ProjectileLaunchSpeed = 5f;
    public float ProjectileCooldown = 3f;
    public Transform ProjectileOrigin;

    public GameObject SlamSnapShot;
    public GameObject DamageBox;
    private Vector3 DamageBoxPos = new Vector3(0, -0.25f, 2.57f);

    public GameObject BossUICanvas;
    public TextMeshProUGUI NameText;
    public Slider Healthbar;

    public GameObject CastBarHolder;
    public TextMeshProUGUI CastNameLabel;
    public Slider Castbar;
    private string Castname = "";

    public ParticleSystem QuakeEffect;

    public float SlamCastTime = 4f;
    public float ClearArenaCastTime = 8f;
    private float CurrentAbilityCastTime = 0f;

    public List<BossOne_BoulderSpawner> BS = new List<BossOne_BoulderSpawner>();
    private int CurrentSpawner = 0;
    public float DefaultPhaseLength = 15f;

    private float CurrentHealth;

    public GameObject Player;

    public GameObject DestroyThisWhenDead;

    private BossOnePhases CurrentPhase = BossOnePhases.Zero;

    private bool CoroutineRunning = false;
    private bool Casting = false;

    private float DefaultPhaseTimer = 0f;
    private float CastTimer = 0f;
    private float Timer = 0f;

    private Animator Anim;

    public enum BossOnePhases
    { 
        Zero,
        Default,
        Slam,
        MegaQuake,
        Hurt,
        Dead,
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = MaxHealth;
        NameText.text = name;
        Anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;

        if (CurrentPhase == BossOnePhases.Zero)
        {
            Healthbar.value = Timer / MaxHealth;
            if(Timer >= MaxHealth)
            {
                CurrentPhase = BossOnePhases.Default;
                Timer = 0;
            }
        }

        if(Casting)
        {
            CastBarHolder.SetActive(true);
            CastTimer += Time.deltaTime;
            CastNameLabel.text = Castname;
            Castbar.value = CastTimer / CurrentAbilityCastTime;

            if(CastTimer >= CurrentAbilityCastTime)
            {
                Casting = false;
                CastTimer = 0;
                CastBarHolder.SetActive(false);
            }
        }

        switch (CurrentPhase)
        {
            case BossOnePhases.Zero:
                break;
            case BossOnePhases.Default:
                DefaultPhaseManager();
                LookAtPlayer();
                break;
            case BossOnePhases.Slam:
                StartCoroutine(Slam());
                break;
            case BossOnePhases.MegaQuake:
                StartCoroutine(MegaQuakePhase());
                break;
            case BossOnePhases.Hurt:
                StartCoroutine(HurtTransition());
                break;
            case BossOnePhases.Dead:
                StartCoroutine(DeathSequence());
                break;
        }
    }

    private void UpdateHealthDisplay()
    {
        Healthbar.value = CurrentHealth / MaxHealth;
    }

    public void DefaultPhaseManager()
    {
        DefaultPhaseTimer += Time.deltaTime;

        if (DefaultPhaseTimer <= DefaultPhaseLength)
        {
            if (Timer >= ProjectileCooldown)
            {
                LaunchProjectile();
                Timer = 0;
            }
        } 
        else
        {
            CurrentPhase = BossOnePhases.Slam;
            DefaultPhaseTimer = 0;
            Timer = 0;
        }
    }

    public void LaunchProjectile()
    {
        Vector3 launchPos = ProjectileOrigin.position;
        Vector3 dir = Player.transform.position - launchPos;

        GameObject obj = Instantiate(Projectile, launchPos, Quaternion.identity);
        obj.GetComponent<Rigidbody>().velocity = dir.normalized * ProjectileLaunchSpeed;
    }

    public IEnumerator Slam()
    {
        if(!CoroutineRunning)
        {
            CoroutineRunning = true;
            transform.rotation = Quaternion.Euler(SlamRandomLookRotation());
            SlamSnapShot.SetActive(true);
            CurrentAbilityCastTime = SlamCastTime;
            Castname = "Casting Seismic Slam";
            Casting = true;
            Anim.SetTrigger("PrepSlam");

            yield return new WaitForSeconds(SlamCastTime);

            SlamSnapShot.SetActive(false);
            DamageBox.transform.localPosition = DamageBoxPos;
            Anim.SetTrigger("Slam");

            yield return new WaitForSeconds(0.25f);

            DamageBox.transform.localPosition = new Vector3(0, -15, 0);

            yield return new WaitForSeconds(1.25f);

            BS[CurrentSpawner].SpawnBoulder();
            CurrentPhase = BossOnePhases.MegaQuake;
            CoroutineRunning = false;
        }
    }

    public IEnumerator HurtTransition()
    {
        yield return new WaitForSeconds(2);

        CurrentPhase = BossOnePhases.Default;
    }

    public IEnumerator MegaQuakePhase()
    {
        if (!CoroutineRunning)
        {
            Anim.SetTrigger("PrepQuake");
            CoroutineRunning = true;
            Castname = "Casting Mega Quake";
            CurrentAbilityCastTime = ClearArenaCastTime;
            Casting = true;

            yield return new WaitForSeconds(ClearArenaCastTime);

            Anim.SetTrigger("Quake");

            yield return new WaitForSeconds(0.3f);

            QuakeEffect.Play();
            HurtPlayer();

            if(BS[CurrentSpawner].Obj != null)
            {
                Destroy(BS[CurrentSpawner].Obj);
            }

            yield return new WaitForSeconds(1.7f);

            CoroutineRunning = false;
            CurrentPhase = BossOnePhases.Default;
        }
    }

    public IEnumerator DeathSequence()
    {
        Anim.SetTrigger("Death");

        yield return new WaitForSeconds(3f);

        Destroy(BossUICanvas);
        Destroy(gameObject);
        Destroy(DestroyThisWhenDead);
    }

    public void LookAtPlayer()
    {
        Quaternion rotation = Quaternion.LookRotation(Player.transform.position);
        rotation.x = 0f;
        rotation.z = 0f;
        transform.rotation = rotation;
    }

    public Vector3 SlamRandomLookRotation()
    {
        int rand = Random.Range(0, 4);
        CurrentSpawner = rand;
        Vector3 rotation = new Vector3(0, rand * 90, 0);
        return rotation;
    }

    public void TakeDamage(int dmg)
    {
        CurrentHealth -= dmg;
        UpdateHealthDisplay();

        if (CurrentHealth == 0)
        {
            CurrentPhase = BossOnePhases.Dead;
        }
        else
        {
            CurrentPhase = BossOnePhases.Hurt;
            Anim.SetTrigger("Hurt");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Boulder")
        {
            TakeDamage(1);
            StopCoroutines();
            Destroy(collision.gameObject);
        }
    }

    public void HurtPlayer()
    {
        Player.GetComponent<Player>().OnTakeDamage(1);
    }

    public void StopCoroutines()
    {
        StopAllCoroutines();
        CoroutineRunning = false;
        Casting = false;
        CastTimer = 0;
        CastBarHolder.SetActive(false);
    }
}
