using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoreMountains.CorgiEngine
{
	public class ButtonPrompt : CorgiMonoBehaviour
	{
		[Header("Bindings")]

		/// the image to use as the border for this prompt
		[Tooltip("the image to use as the border for this prompt")]
		public Image Border;
		/// the background image of the prompt
		[Tooltip("the background image of the prompt")]
		public Image Background;
		/// the canvas group containing the prompt
		[Tooltip("the canvas group containing the prompt")]
		public CanvasGroup ContainerCanvasGroup;
		/// the text component displaying this prompt's contents
		[Tooltip("the text component displaying this prompt's contents")]
		public Text PromptText;

		[Header("Durations")]

		/// the duration of the prompt's fade in, in seconds
		[Tooltip("the duration of the prompt's fade in, in seconds")]
		public float FadeInDuration = 0.2f;
		/// the duration of the prompt's fade out, in seconds
		[Tooltip("the duration of the prompt's fade out, in seconds")]
		public float FadeOutDuration = 0.2f;
        
		protected Color _alphaZero = new Color(1f, 1f, 1f, 0f);
		protected Color _alphaOne = new Color(1f, 1f, 1f, 1f);
		protected Coroutine _hideCoroutine;

		protected Color _tempColor;

		public virtual void Initialization()
		{
			ContainerCanvasGroup.alpha = 0f;
		}

		public virtual void SetText(string newText)
		{
			PromptText.text = newText;
		}

		public virtual void SetBackgroundColor(Color newColor)
		{
			Background.color = newColor;
		}

		public virtual void SetTextColor(Color newColor)
		{
			PromptText.color = newColor;
		}

		public virtual void Show()
		{
			this.gameObject.SetActive(true);
			if (_hideCoroutine != null)
			{
				StopCoroutine(_hideCoroutine);
			}
			ContainerCanvasGroup.alpha = 0f;
			StartCoroutine(MMFade.FadeCanvasGroup(ContainerCanvasGroup, FadeInDuration, 1f, true));
		}

		public virtual void Hide()
		{
			_hideCoroutine = StartCoroutine(HideCo());
		}

		protected virtual IEnumerator HideCo()
		{
			ContainerCanvasGroup.alpha = 1f;
			StartCoroutine(MMFade.FadeCanvasGroup(ContainerCanvasGroup, FadeOutDuration, 0f, true));
			yield return new WaitForSeconds(FadeOutDuration);
			this.gameObject.SetActive(false);
		}
	}
}