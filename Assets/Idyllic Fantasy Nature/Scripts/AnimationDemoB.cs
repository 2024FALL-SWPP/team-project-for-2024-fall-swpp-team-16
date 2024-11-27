using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK
{
    public class AnimationDemoB : MonoBehaviour
    {

        public enum AnimationTypeB
        {
            Trigger,
            Bool
        }

        [System.Serializable]
        public class AnimationEntryB
        {
            public string animationNameB;
            public AnimationTypeB typeB;
        }

        public List<AnimationEntryB> entriesB = new List<AnimationEntryB>();

        public List<Animator> animatorsB = new List<Animator>();

        public int entryIndexB;
        public Text animationNameTextB;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                NextAnimationB();
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                ReplayAnimationB();
            }
        }

        public void NextAnimationB()
        {
            entryIndexB++;
            if (entriesB.Count - 1 < entryIndexB) entryIndexB = 0;
            animationNameTextB.text = entriesB[entryIndexB].animationNameB;
            PlayAnimationB();
        }

        public void PreviousAnimationB()
        {
            entryIndexB--;
            if (entryIndexB < 0) entryIndexB = entriesB.Count - 1;
            animationNameTextB.text = entriesB[entryIndexB].animationNameB;
            PlayAnimationB();
        }

        public void ReplayAnimationB()
        {
            PlayAnimationB();
        }

        private void ResetAllBoolB()
        {
            foreach (var entryB in entriesB)
            {
                if (entryB.typeB != AnimationTypeB.Bool) continue;
                foreach (var animatorB in animatorsB)
                {
                    animatorB.SetBool(entryB.animationNameB, false);
                }
            }
        }

        private void PlayAnimationB()
        {
            ResetAllBoolB();

            if (entriesB[entryIndexB].typeB == AnimationTypeB.Bool)
            {
                foreach (var animatorB in animatorsB)
                {
                    animatorB.SetBool(entriesB[entryIndexB].animationNameB, true);
                }
            }
            else
            {
                foreach (var animatorB in animatorsB)
                {
                    animatorB.SetTrigger(entriesB[entryIndexB].animationNameB);
                }
            }
        }
    }
}
