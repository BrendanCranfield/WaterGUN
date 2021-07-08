using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
	public class QuickOptionsPopup : BasePopup
	{
		[SerializeField] private MultiInputSlider m_MasterVolumeSlider = null;
		[SerializeField] private MultiInputSlider m_MusicVolumeSlider = null;
		[SerializeField] private MultiInputSlider m_HorizontalSensitivitySlider = null;
		[SerializeField] private MultiInputSlider m_VerticalSensitivitySlider = null;
		[SerializeField] private MultiInputToggle m_InvertMouseToggle = null;
		[SerializeField] private MultiInputMultiChoice m_VsyncChoice = null;

		private static QuickOptionsPopup m_Instance = null;

		private bool m_AudioSettingsChanged = false;
		private bool m_GraphicsSettingsChanged = false;
		private bool m_InputSettingsChanged = false;

		public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);
			m_Instance = this;

			// Add listeners from code (saves user doing it as prefabs have a tendency to break)
			if (m_MasterVolumeSlider != null)
				m_MasterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
			if (m_MusicVolumeSlider != null)
				m_MusicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
			if (m_VsyncChoice != null)
				m_VsyncChoice.onIndexChanged.AddListener(OnVsyncChanged);
			if (m_HorizontalSensitivitySlider != null)
				m_HorizontalSensitivitySlider.onValueChanged.AddListener(OnHorizontalSensitivityChanged);
			if (m_VerticalSensitivitySlider != null)
				m_VerticalSensitivitySlider.onValueChanged.AddListener(OnVerticalSensitivityChanged);
			if (m_InvertMouseToggle != null)
				m_InvertMouseToggle.onValueChanged.AddListener(OnInvertMouseChanged);
		}

		void OnDestroy ()
		{
			if (m_Instance == this)
				m_Instance = null;
		}

		public override void Back ()
		{
			OnOK ();
		}

		public void OnOK ()
		{
			if (m_AudioSettingsChanged)
				FpsSettings.audio.Save();
			if (m_GraphicsSettingsChanged)
				FpsSettings.graphics.Save();
			if (m_InputSettingsChanged)
				FpsSettings.input.Save();

			m_Instance.menu.ShowPopup (null);
		}

		public static void ToggleVisible ()
		{
			if (m_Instance != null)
			{
				if (m_Instance.isActiveAndEnabled)
					m_Instance.OnOK();
				else
					m_Instance.ShowPopupInternal();
			}
		}

		void ShowPopupInternal()
		{
			m_AudioSettingsChanged = false;
			m_GraphicsSettingsChanged = false;
			m_InputSettingsChanged = false;

			if (m_MasterVolumeSlider != null)
				m_MasterVolumeSlider.value = Mathf.RoundToInt(FpsSettings.audio.masterVolume * 100f);
			if (m_MusicVolumeSlider != null)
				m_MusicVolumeSlider.value = Mathf.RoundToInt(FpsSettings.audio.musicVolume * 100f);
			if (m_VsyncChoice != null)
				m_VsyncChoice.index = FpsSettings.graphics.vSync;
			if (m_InvertMouseToggle != null)
				m_InvertMouseToggle.value = FpsSettings.input.invertMouse;
			if (m_HorizontalSensitivitySlider != null)
			{
				int current = Mathf.RoundToInt(FpsSettings.input.horizontalMouseSensitivity * 100f);
				m_HorizontalSensitivitySlider.value = current;
			}
			if (m_VerticalSensitivitySlider != null)
			{
				int current = Mathf.RoundToInt(FpsSettings.input.verticalMouseSensitivity * 100f);
				m_VerticalSensitivitySlider.value = current;
			}

			menu.ShowPopup(this);
		}

		public void OnVsyncChanged(int index)
		{
			FpsSettings.graphics.vSync = index;
			m_GraphicsSettingsChanged = true;
		}

		public void OnMasterVolumeChanged(int value)
		{
			FpsSettings.audio.masterVolume = (float)value * 0.01f;
			m_AudioSettingsChanged = true;
		}

		public void OnMusicVolumeChanged(int value)
		{
			FpsSettings.audio.musicVolume = (float)value * 0.01f;
			m_AudioSettingsChanged = true;
		}
		public void OnHorizontalSensitivityChanged(int value)
		{
			FpsSettings.input.horizontalMouseSensitivity = (float)value * 0.01f;
			m_InputSettingsChanged = true;
		}

		public void OnVerticalSensitivityChanged(int value)
		{
			FpsSettings.input.verticalMouseSensitivity = (float)value * 0.01f;
			m_InputSettingsChanged = true;
		}
		public void OnInvertMouseChanged(bool value)
		{
			FpsSettings.input.invertMouse = value;
			m_InputSettingsChanged = true;
		}
	}
}