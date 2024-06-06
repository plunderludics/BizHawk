﻿using System.Collections.Generic;
using System.IO;
using BizHawk.Emulation.Common;

namespace BizHawk.Client.Common
{
	public interface IMovieSession
	{
		IMovieConfig Settings { get; }
		IMovie Movie { get; }
		bool ReadOnly { get; set; }

		/// <summary>
		/// Gets a value indicating whether or not a new movie is queued for loading
		/// </summary>
		bool NewMovieQueued { get; }

		/// <summary>
		/// Gets the sync settings from a queued movie, if a movie is queued
		/// </summary>
		string QueuedSyncSettings { get; }

		/// <value>The Core header of the queued movie iff one is queued, else <see langword="null"/></value>
		string QueuedCoreName { get; }

		IDictionary<string, object> UserBag { get; set; }

		IMovieController MovieController { get; }

		/// <summary>
		/// Provides a source for sticky controls ot use when recording
		/// </summary>
		IStickyAdapter StickySource { get; set; }

		/// <summary>
		/// Represents the input source that is fed to
		/// the movie for the purpose of recording, if active,
		/// or to simply pass through if inactive
		/// </summary>
		IInputAdapter MovieIn { get; set; }

		/// <summary>
		/// Represents the movie input in the input chain
		/// Is a pass through when movies are not active,
		/// otherwise they handle necessary movie logic
		/// </summary>
		IInputAdapter MovieOut { get; }

		/// <summary>
		/// Creates a <see cref="IMovieController" /> instance based on the
		/// given button definition if provided else the
		/// current <see cref="MovieController" /> button definition
		/// will be used
		/// </summary>
		IMovieController GenerateMovieController(ControllerDefinition definition = null);

		/// <summary>
		/// Hack only used for TAStudio when starting a new movie
		/// This is due to needing to save a "dummy" default.tasproj
		/// This dummy file's initial save bypasses the normal queue/run
		/// new movie code (which normally sets the controller), although
		/// once it saves it goes through the normal queue/run code anyway.
		/// TODO: Stop relying on this dummy file so we do not need this ugly hack
		/// </summary>
		/// <param name="definition">current IEmulator ControllerDefinition</param>
		void SetMovieController(ControllerDefinition definition);

		void HandleFrameBefore();
		void HandleFrameAfter();
		void HandleSaveState(TextWriter writer);

		bool CheckSavestateTimeline(TextReader reader);
		bool HandleLoadState(TextReader reader);

		/// <summary>
		/// Queues up a movie for loading
		/// When initializing a movie, it will be stored until Rom loading processes have been completed, then it will be moved to the Movie property
		/// If an existing movie is still active, it will remain in the Movie property while the new movie is queued
		/// </summary>
		void QueueNewMovie(IMovie movie, bool record, string systemId, IDictionary<string, string> preferredCores);

		/// <summary>
		/// Sets the Movie property with the QueuedMovie, clears the queued movie, and starts the new movie
		/// </summary>
		void RunQueuedMovie(bool recordMode, IEmulator emulator);

		/// <summary>clears the queued movie</summary>
		void AbortQueuedMovie();

		void StopMovie(bool saveChanges = true);

		/// <summary>
		/// If a movie is active, it will be converted to a <see cref="ITasMovie" />
		/// </summary>
		void ConvertToTasProj();

		IMovie Get(string path);

		string BackupDirectory { get; set; }

		void PopupMessage(string message);
	}
}
