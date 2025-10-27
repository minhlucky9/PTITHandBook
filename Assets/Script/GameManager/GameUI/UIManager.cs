using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameManager
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Window")]
        public UIAnimationController hudWindow;

        public void OpenCommentGoogleForm()
        {
            Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSfNynGQccbdIsXf2bIIz8KR03o7ej0N2gKkpKCG1FuZFx-GZA/viewform?usp=header");
        }


    }
}
