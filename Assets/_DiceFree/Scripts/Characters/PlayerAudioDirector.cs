using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Characters
{
    /// <summary>
    /// First curated RPG sound pass. Separate low-volume ambient, effects and
    /// footstep sources, intentionally subtle compared with later classes.
    /// Only the locally selected, versioned Audio resources are loaded.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAudioDirector : MonoBehaviour, IPlayerAudioSettings
    {
        [SerializeField, Range(0f,1f)] private float effectsVolume=.27f;
        [SerializeField, Range(0f,1f)] private float ambientVolume=.12f;
        [SerializeField, Range(0f,1f)] private float footstepsVolume=.14f;
        private AudioSource effects,footsteps,ambience;
        private float sfxGain=1f, ambientGain=1f;
        public float SfxLevel => sfxGain;
        public float AmbientLevel => ambientGain;

        public void SetLevels(float sfx,float ambient)
        {
            sfxGain=Mathf.Clamp01(sfx);
            ambientGain=Mathf.Clamp01(ambient);
            if(effects!=null)effects.volume=effectsVolume*sfxGain;
            if(footsteps!=null)footsteps.volume=footstepsVolume*sfxGain;
            if(ambience!=null)ambience.volume=ambientVolume*ambientGain;
            PlayerPrefs.SetFloat("DiceFree.Audio.Sfx",sfxGain);
            PlayerPrefs.SetFloat("DiceFree.Audio.Ambient",ambientGain);
        }
        private Health health;
        private BasicAttack basicAttack;
        private NoviceSkillCaster novice;
        private MagicalSkillCaster magical;
        private PhysicalSkillCaster physical;
        private ExperienceProgression experience;
        private int previousHits;
        private Vector3 oldPosition;
        private float nextStep;
        private static readonly System.Collections.Generic.Dictionary<string,AudioClip> Loaded=new();

        public bool AudioConfigured => effects!=null && ambience!=null &&
            footsteps!=null && Get("StepGrass")!=null;
        public bool AmbientPlaying => ambience!=null && ambience.isPlaying;

        private static AudioClip Get(string key)
        {
            if(Loaded.TryGetValue(key,out var clip))return clip;
            clip=Resources.Load<AudioClip>("Audio/"+key);
            Loaded[key]=clip;
            return clip;
        }

        private static AudioSource CreateSource(GameObject owner,string name,bool loop,float volume)
        {
            var audio=owner.AddComponent<AudioSource>();
            audio.playOnAwake=false;
            audio.loop=loop;
            audio.volume=volume;
            audio.spatialBlend=0f;
            audio.dopplerLevel=0f;
            return audio;
        }

        private void Awake()
        {
            effects=CreateSource(gameObject,"DiceFree Effects",false,effectsVolume);
            footsteps=CreateSource(gameObject,"DiceFree Footsteps",false,footstepsVolume);
            ambience=CreateSource(gameObject,"DiceFree Ambient",true,ambientVolume);
            ambience.clip=Get("CornbergBirds");
            SetLevels(PlayerPrefs.GetFloat("DiceFree.Audio.Sfx",1f),
                PlayerPrefs.GetFloat("DiceFree.Audio.Ambient",1f));
            health=GetComponent<Health>();
            basicAttack=GetComponent<BasicAttack>();
            novice=GetComponent<NoviceSkillCaster>();
            magical=GetComponent<MagicalSkillCaster>();
            physical=GetComponent<PhysicalSkillCaster>();
            experience=GetComponent<ExperienceProgression>();
            oldPosition=transform.position;
            previousHits=basicAttack!=null?basicAttack.Hits:0;
        }

        private void OnEnable()
        {
            if(health!=null){health.Damaged+=OnDamaged;health.Healed+=OnHealed;}
            if(novice!=null)novice.Casted+=OnNovice;
            if(magical!=null)magical.Casted+=OnMagical;
            if(physical!=null)physical.Casted+=OnPhysical;
            if(experience!=null)experience.LeveledUp+=OnLeveled;
            if(ambience!=null&&ambience.clip!=null&&!ambience.isPlaying)
                ambience.Play();
        }

        private void OnDisable()
        {
            if(health!=null){health.Damaged-=OnDamaged;health.Healed-=OnHealed;}
            if(novice!=null)novice.Casted-=OnNovice;
            if(magical!=null)magical.Casted-=OnMagical;
            if(physical!=null)physical.Casted-=OnPhysical;
            if(experience!=null)experience.LeveledUp-=OnLeveled;
            if(ambience!=null)ambience.Stop();
        }

        public void Play(string id,float scale=1f)
        {
            var clip=Get(id);
            if(clip==null||effects==null)return;
            effects.PlayOneShot(clip,Mathf.Clamp01(scale));
        }

        private void Update()
        {
            if(basicAttack!=null)
            {
                if(basicAttack.Hits>previousHits)
                    Play("WeaponImpact",.55f);
                previousHits=basicAttack.Hits;
                if(basicAttack.State=="Wind-up" && Time.time>=nextSwing)
                {
                    Play("WeaponSwing",.20f);
                    nextSwing=Time.time+.35f;
                }
            }
            // Distance, rather than a fixed timer, makes steps respond to slow,
            // walking, running, and direct WASD movement without animation events.
            float distance=Vector3.Distance(transform.position,oldPosition);
            oldPosition=transform.position;
            if(distance>.015f && Time.unscaledTime>=nextStep &&
                health!=null && health.Alive)
            {
                nextStep=Time.unscaledTime+.37f;
                var clip=Get("StepGrass");
                if(clip!=null && footsteps!=null)
                    footsteps.PlayOneShot(clip,1f);
            }
        }
        private float nextSwing;

        private void OnDamaged(CombatActor _,DamageResult result)
        {
            if(result.applied>0)Play("PlayerHurt",.42f);
        }
        private void OnHealed(float amount)
        {
            if(amount>0)Play("Heal",.55f);
        }
        private void OnLeveled(int _) => Play("LevelUp",.45f);

        private void OnNovice(NoviceSkillDefinition def)
        {
            if(def==null)return;
            switch(def.kind)
            {
                case NoviceSkillKind.StrengthMeleeStun:Play("WeaponSwing",.62f);break;
                case NoviceSkillKind.MagicSand:Play("MagicSand",.38f);break;
                case NoviceSkillKind.AgilityAttackSpeedBuff:Play("Buff",.32f);break;
                case NoviceSkillKind.SpiritHeal:break; // Health.Healed plays once.
            }
        }
        private void OnMagical(MagicalSkillDefinition def)
        {
            if(def==null)return;
            switch(def.kind)
            {
                case MagicalSkillKind.MagicSand:Play("MagicSand",.48f);break;
                case MagicalSkillKind.Mend:break; // Health.Healed plays once.
                case MagicalSkillKind.FireImbuement:Play("MagicFire",.30f);break;
                case MagicalSkillKind.IceBurst:Play("MagicIce",.45f);break;
            }
        }
        private void OnPhysical(PhysicalSkillDefinition def)
        {
            if(def==null)return;
            switch(def.kind)
            {
                case PhysicalSkillKind.HeavyStrike:Play("WeaponSwing",.67f);break;
                case PhysicalSkillKind.ArrowRain:Play("WeaponSwing",.42f);break;
                case PhysicalSkillKind.Guard:Play("Buff",.40f);break;
                case PhysicalSkillKind.Quickening:Play("Buff",.40f);break;
            }
        }
    }
}
