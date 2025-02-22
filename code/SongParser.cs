using Sandbox;
using Sandbox.Audio;

namespace Kiru;
public sealed class SongParser : Component
{
	[Property] public float ScrollSpeed { get; set; } = 1500;
	[Property] GameObject NotePrefab { get; set; }
	[Property] public float SpawnDistance { get; set; } = 1024;
	[Property] private GameObject StartLine { get; set; }
	[Property] public Color LeftNoteColor { get; set; } = Color.Red;
	[Property] public Color RightNoteColor { get; set; } = Color.Cyan;
	[Property] public SoundEvent HitSound { get; set; }
	[Property] public SoundEvent MissSound { get; set; }
	public static SoundEvent HitSoundEvent { get; set; }
	public static SoundEvent MissSoundEvent { get; set; }
	[RequireComponent] SongBrowser Browser { get; set; }
	SongChart SongChartData { get; set; }
	SongInfo songInfo { get; set; }
	MusicPlayer musicPlayer { get; set; }
	Grid grid { get; set; }
	float currentBeat { get; set; }
	public static bool IsSongPlaying { get; set; }
	int noteCount { get; set; } = 0;
	public float BPM { get; set; }
	public float timeToReach { get; set; } = 0;
	protected override void OnStart()
	{
		HitSoundEvent = HitSound;
		MissSoundEvent = MissSound;
		MissSound = MissSoundEvent;
		grid = Scene.Components.GetInChildren<Grid>();
		IsSongPlaying = false;
	}
	protected override void OnFixedUpdate()
	{
		if( IsSongPlaying && SongChartData?.Notes != null )
		{
			currentBeat += BPM / 60 * Time.Delta;
			var currentNote = SongChartData.Notes[noteCount];
			timeToReach = Vector3.DistanceBetween( grid.WorldPosition, Vector3.Zero.WithX( StartLine.WorldPosition.x ) ) / ScrollSpeed * BPM / 60;
			if( currentBeat >= currentNote.Time - timeToReach )
			{
				//Spawn note prefab, set position, and pass note data
				GameObject no = NotePrefab.Clone( grid.Positions[currentNote.LineIndex][currentNote.LineLayer].WorldPosition );
				no.Name = $"note_{noteCount.ToString()}";
				NoteComponent co = no.GetComponentInChildren<NoteComponent>();
				co.Parser = this;
				co.noteData = currentNote;
				co.NoteSpeed = ScrollSpeed;
				noteCount++;
			}
		}
	}

	protected override void OnUpdate()
	{
		if ( Game.IsRunningInVR )
		{
			if ( Input.VR.LeftHand.ButtonB.WasPressed || Input.VR.RightHand.ButtonB.WasPressed ) Input.EscapePressed = true;
		}
		if ( Input.EscapePressed && SongChartData != null )
		{
			IsSongPlaying = !IsSongPlaying;
			musicPlayer.Paused = !musicPlayer.Paused;
			Browser.Enabled = !Browser.Enabled;
		}
	}
	public void PlaySong(SongChart data, SongInfo info, string audio)
	{
		IsSongPlaying = false;
		Browser.Enabled = false;
		SongChartData = data;
		songInfo = info;
		currentBeat = 0;
		BPM = songInfo.BPM;
		noteCount = 0;
		
		foreach( var i in Scene.Components.GetAll<NoteComponent>() ) i.GameObject.Destroy();
		musicPlayer = MusicPlayer.Play( FileSystem.Data, audio );
		musicPlayer.TargetMixer = Mixer.FindMixerByName( "Music" );
		musicPlayer.ListenLocal = true;

		IsSongPlaying = true;
	}
}
