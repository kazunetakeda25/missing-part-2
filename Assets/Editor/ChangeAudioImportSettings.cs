

using UnityEngine;
using UnityEditor;

// /////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Batch audio import settings modifier.
//
// Modifies all selected audio clips in the project window and applies the requested modification on the
// audio clips. Idea was to have the same choices for multiple files as you would have if you open the
// import settings of a single audio clip. Put this into Assets/Editor and once compiled by Unity you find
// the new functionality in Custom -> Sound. Enjoy! :-)
//
// April 2010. Based on Martin Schultz's texture import settings batch modifier.
//
// /////////////////////////////////////////////////////////////////////////////////////////////////////////
public class ChangeAudioImportSettings : ScriptableObject {

	[MenuItem ("Custom/Sound/Toggle audio compression/Disable")]
	static void ToggleCompression_Disable() {
		SelectedToggleCompressionSettings(AudioImporterFormat.Native);
	}

	[MenuItem ("Custom/Sound/Toggle audio compression/Enable")]
	static void ToggleCompression_Enable() {
		SelectedToggleCompressionSettings(AudioImporterFormat.Compressed);
	}

	// ----------------------------------------------------------------------------

	[MenuItem ("Custom/Sound/Set audio compression bitrate (kbps)/32")]
	static void SetCompressionBitrate_32kbps() {
		SelectedSetCompressionBitrate(32000);
	}

	[MenuItem ("Custom/Sound/Set audio compression bitrate (kbps)/64")]
	static void SetCompressionBitrate_64kbps() {
		SelectedSetCompressionBitrate(64000);
	}

	[MenuItem ("Custom/Sound/Set audio compression bitrate (kbps)/96")]
	static void SetCompressionBitrate_96kbps() {
		SelectedSetCompressionBitrate(96000);
	}

	[MenuItem ("Custom/Sound/Set audio compression bitrate (kbps)/128")]
	static void SetCompressionBitrate_128kbps() {
		SelectedSetCompressionBitrate(128000);
	}

	[MenuItem ("Custom/Sound/Set audio compression bitrate (kbps)/144")]
	static void SetCompressionBitrate_144kbps() {
		SelectedSetCompressionBitrate(144000);
	}

	[MenuItem ("Custom/Sound/Set audio compression bitrate (kbps)/156 (default)")]
	static void SetCompressionBitrate_156kbps() {
		SelectedSetCompressionBitrate(156000);
	}

	[MenuItem ("Custom/Sound/Set audio compression bitrate (kbps)/160")]
	static void SetCompressionBitrate_160kbps() {
		SelectedSetCompressionBitrate(160000);
	}

	[MenuItem ("Custom/Sound/Set audio compression bitrate (kbps)/192")]
	static void SetCompressionBitrate_192kbps() {
		SelectedSetCompressionBitrate(192000);
	}

	[MenuItem ("Custom/Sound/Set audio compression bitrate (kbps)/224")]
	static void SetCompressionBitrate_224kbps() {
		SelectedSetCompressionBitrate(224000);
	}

	[MenuItem ("Custom/Sound/Set audio compression bitrate (kbps)/240")]
	static void SetCompressionBitrate_240kbps() {
		SelectedSetCompressionBitrate(240000);
	}

	// ----------------------------------------------------------------------------

	[MenuItem ("Custom/Sound/load type/Stream from disc")]
	static void ToggleDecompressOnLoad_Disable() {
		SelectedToggleDecompressOnLoadSettings(AudioClipLoadType.Streaming);
	}

	[MenuItem ("Custom/Sound/load type/Descompress on Load")]
	static void ToggleDecompressOnLoad_Enable() {
		SelectedToggleDecompressOnLoadSettings(AudioClipLoadType.DecompressOnLoad);
	}

	[MenuItem ("Custom/Sound/load type/CompressedInMemory")]
	static void ToggleDecompressOnLoad_Enable2() {
		SelectedToggleDecompressOnLoadSettings(AudioClipLoadType.CompressedInMemory);
	}

	// ----------------------------------------------------------------------------

	[MenuItem ("Custom/Sound/Toggle 3D sound/Disable")]
	static void Toggle3DSound_Disable() {
		SelectedToggle3DSoundSettings(false);
	}

	[MenuItem ("Custom/Sound/Toggle 3D sound/Enable")]
	static void Toggle3DSound_Enable() {
		SelectedToggle3DSoundSettings(true);
	}

	// ----------------------------------------------------------------------------

	[MenuItem ("Custom/Sound/Toggle mono/Auto")]
	static void ToggleForceToMono_Auto() {
		SelectedToggleForceToMonoSettings(false);
	}

	[MenuItem ("Custom/Sound/Toggle mono/Forced")]
	static void ToggleForceToMono_Forced() {
		SelectedToggleForceToMonoSettings(true);
	}

	// ----------------------------------------------------------------------------
	[MenuItem ("Custom/Sound/Hardware Decoding/Enabled")]
	static void enable_Hardware_yes() {
		enableHardwareDecoding(true);
	}
	[MenuItem ("Custom/Sound/Hardware Decoding/Disabled")]
	static void enable_Hardware_no() {
		enableHardwareDecoding(false);
	}



	static void enableHardwareDecoding ( bool enable )
	{
		Object[] audioclips = GetSelectedAudioclips();
		Selection.objects = new Object[0];
		foreach (AudioClip audioclip in audioclips) {
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			audioImporter.hardware = enable;
			AssetDatabase.ImportAsset(path);
		}
	}

	static void SelectedToggleCompressionSettings(AudioImporterFormat newFormat) {

		Object[] audioclips = GetSelectedAudioclips();
		Selection.objects = new Object[0];
		foreach (AudioClip audioclip in audioclips) {
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
#if UNITY_5_0_OR_LOWER
			audioImporter.format = newFormat;
#endif
			AssetDatabase.ImportAsset(path);
		}
	}

	static void SelectedSetCompressionBitrate(float newCompressionBitrate) {

		Object[] audioclips = GetSelectedAudioclips();
		Selection.objects = new Object[0];
		foreach (AudioClip audioclip in audioclips) {
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
#if UNITY_5_0_OR_LOWER
			audioImporter.compressionBitrate = (int)newCompressionBitrate;
#endif
			AssetDatabase.ImportAsset(path);
		}
	}

	static void SelectedToggleDecompressOnLoadSettings(AudioClipLoadType enabled) {

		Object[] audioclips = GetSelectedAudioclips();
		Selection.objects = new Object[0];
		foreach (AudioClip audioclip in audioclips) {
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			AudioImporterSampleSettings audioImporterSampleSettings = audioImporter.defaultSampleSettings;
			audioImporterSampleSettings.loadType = enabled;
			audioImporter.defaultSampleSettings = audioImporterSampleSettings;
			AssetDatabase.ImportAsset(path);
		}
	}

	static void SelectedToggle3DSoundSettings(bool enabled) {

		Object[] audioclips = GetSelectedAudioclips();
		Selection.objects = new Object[0];
		foreach (AudioClip audioclip in audioclips) {
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			audioImporter.threeD = enabled;
			AssetDatabase.ImportAsset(path);
		}
	}

	static void SelectedToggleForceToMonoSettings(bool enabled) {

		Object[] audioclips = GetSelectedAudioclips();
		Selection.objects = new Object[0];
		foreach (AudioClip audioclip in audioclips) {
			string path = AssetDatabase.GetAssetPath(audioclip);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			audioImporter.forceToMono = enabled;
			AssetDatabase.ImportAsset(path);
		}
	}

	static Object[] GetSelectedAudioclips()
	{
		return Selection.GetFiltered(typeof(AudioClip), SelectionMode.DeepAssets);
	}
}
