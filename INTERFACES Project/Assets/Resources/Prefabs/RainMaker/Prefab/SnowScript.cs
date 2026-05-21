using UnityEngine;
using DigitalRuby.RainMaker;

namespace DigitalRuby.RainMaker
{
    public class SnowScript : BaseRainScript
    {
        protected override void Start()
        {
            base.Start();
            ApplySnowSettings();
        }

        private void ApplySnowSettings()
        {
            // Vent léger pour la neige
            WindSpeedRange = new Vector3(5.0f, 50.0f, 100.0f);
            WindChangeInterval = new Vector2(10.0f, 60.0f);
            RainMistThreshold = 0.2f;

            if (RainFallParticleSystem != null)
            {
                var main = RainFallParticleSystem.main;

                // Chute lente
                main.gravityModifier = 0.05f;
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.0f);

                // Durée de vie longue
                main.startLifetime = new ParticleSystem.MinMaxCurve(4.0f, 8.0f);

                // Rotation aléatoire pour flocons
                var rotationModule = RainFallParticleSystem.rotationOverLifetime;
                rotationModule.enabled = true;
                rotationModule.z = new ParticleSystem.MinMaxCurve(-45f, 45f);
            }
        }

        protected override float RainFallEmissionRate()
        {
            // Flocons plus gros selon l'intensité
            if (RainFallParticleSystem != null)
            {
                var main = RainFallParticleSystem.main;
                float minSize = Mathf.Lerp(0.05f, 0.2f, RainIntensity);
                float maxSize = Mathf.Lerp(0.1f, 0.5f, RainIntensity);
                main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            }

            return (RainFallParticleSystem.main.maxParticles / RainFallParticleSystem.main.startLifetime.constant) * RainIntensity;
        }

        protected override bool UseRainMistSoftParticles
        {
            get { return false; }
        }
    }
}
