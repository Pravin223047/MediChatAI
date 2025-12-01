using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Core.Services.State;

/// <summary>
/// State management for active consultation sessions
/// </summary>
public class ConsultationState
{
    // Current session
    public ConsultationSessionDto? CurrentSession { get; private set; }
    public List<ConsultationParticipantDto> Participants { get; private set; } = new();
    public List<ConsultationNoteDto> Notes { get; private set; } = new();

    // Recording state
    public bool IsRecording { get; private set; }
    public DateTime? RecordingStartTime { get; private set; }
    public TimeSpan RecordingDuration => IsRecording && RecordingStartTime.HasValue
        ? DateTime.UtcNow - RecordingStartTime.Value
        : TimeSpan.Zero;

    // Connection state
    public bool IsConnected { get; private set; }
    public Dictionary<string, bool> ParticipantOnlineStatus { get; private set; } = new();

    // Media state
    public bool IsAudioEnabled { get; private set; } = true;
    public bool IsVideoEnabled { get; private set; } = true;
    public bool IsScreenSharing { get; private set; }

    // Notes
    public string DraftNotes { get; private set; } = string.Empty;

    // Events
    public event Action? OnStateChanged;
    public event Action<ConsultationSessionDto>? OnSessionUpdated;
    public event Action<ConsultationParticipantDto>? OnParticipantJoined;
    public event Action<int>? OnParticipantLeft;
    public event Action<bool>? OnRecordingStateChanged;
    public event Action<bool>? OnConnectionStateChanged;

    // Session management
    public void SetCurrentSession(ConsultationSessionDto session)
    {
        CurrentSession = session;
        OnSessionUpdated?.Invoke(session);
        NotifyStateChanged();
    }

    public void UpdateSession(ConsultationSessionDto session)
    {
        CurrentSession = session;
        OnSessionUpdated?.Invoke(session);
        NotifyStateChanged();
    }

    public void ClearSession()
    {
        CurrentSession = null;
        Participants.Clear();
        Notes.Clear();
        ParticipantOnlineStatus.Clear();
        IsRecording = false;
        RecordingStartTime = null;
        IsConnected = false;
        DraftNotes = string.Empty;
        NotifyStateChanged();
    }

    // Participant management
    public void AddParticipant(ConsultationParticipantDto participant)
    {
        if (!Participants.Any(p => p.Id == participant.Id))
        {
            Participants.Add(participant);
            ParticipantOnlineStatus[participant.UserId ?? participant.Id.ToString()] = participant.IsOnline;
            OnParticipantJoined?.Invoke(participant);
            NotifyStateChanged();
        }
    }

    public void RemoveParticipant(int participantId)
    {
        var participant = Participants.FirstOrDefault(p => p.Id == participantId);
        if (participant != null)
        {
            Participants.Remove(participant);
            var key = participant.UserId ?? participant.Id.ToString();
            ParticipantOnlineStatus.Remove(key);
            OnParticipantLeft?.Invoke(participantId);
            NotifyStateChanged();
        }
    }

    public void UpdateParticipantOnlineStatus(int participantId, bool isOnline)
    {
        var participant = Participants.FirstOrDefault(p => p.Id == participantId);
        if (participant != null)
        {
            participant.IsOnline = isOnline;
            var key = participant.UserId ?? participant.Id.ToString();
            ParticipantOnlineStatus[key] = isOnline;
            NotifyStateChanged();
        }
    }

    public void SetParticipants(List<ConsultationParticipantDto> participants)
    {
        Participants = participants;
        ParticipantOnlineStatus.Clear();
        foreach (var p in participants)
        {
            ParticipantOnlineStatus[p.UserId ?? p.Id.ToString()] = p.IsOnline;
        }
        NotifyStateChanged();
    }

    // Recording management
    public void StartRecording()
    {
        IsRecording = true;
        RecordingStartTime = DateTime.UtcNow;
        OnRecordingStateChanged?.Invoke(true);
        NotifyStateChanged();
    }

    public void StopRecording()
    {
        IsRecording = false;
        RecordingStartTime = null;
        OnRecordingStateChanged?.Invoke(false);
        NotifyStateChanged();
    }

    public void SetRecordingState(bool isRecording)
    {
        if (isRecording && !IsRecording)
        {
            StartRecording();
        }
        else if (!isRecording && IsRecording)
        {
            StopRecording();
        }
    }

    // Connection management
    public void SetConnectionState(bool isConnected)
    {
        if (IsConnected != isConnected)
        {
            IsConnected = isConnected;
            OnConnectionStateChanged?.Invoke(isConnected);
            NotifyStateChanged();
        }
    }

    // Media state
    public void SetAudioState(bool enabled)
    {
        IsAudioEnabled = enabled;
        NotifyStateChanged();
    }

    public void SetVideoState(bool enabled)
    {
        IsVideoEnabled = enabled;
        NotifyStateChanged();
    }

    public void SetScreenSharingState(bool isSharing)
    {
        IsScreenSharing = isSharing;
        NotifyStateChanged();
    }

    // Notes management
    public void UpdateDraftNotes(string content)
    {
        DraftNotes = content;
        NotifyStateChanged();
    }

    public void AddNote(ConsultationNoteDto note)
    {
        if (!Notes.Any(n => n.Id == note.Id))
        {
            Notes.Add(note);
            NotifyStateChanged();
        }
    }

    public void UpdateNote(ConsultationNoteDto note)
    {
        var existing = Notes.FirstOrDefault(n => n.Id == note.Id);
        if (existing != null)
        {
            var index = Notes.IndexOf(existing);
            Notes[index] = note;
            NotifyStateChanged();
        }
    }

    public void RemoveNote(int noteId)
    {
        var note = Notes.FirstOrDefault(n => n.Id == noteId);
        if (note != null)
        {
            Notes.Remove(note);
            NotifyStateChanged();
        }
    }

    public void SetNotes(List<ConsultationNoteDto> notes)
    {
        Notes = notes;
        NotifyStateChanged();
    }

    // Notifications
    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    // Utility
    public int GetOnlineParticipantCount()
    {
        return ParticipantOnlineStatus.Count(kvp => kvp.Value);
    }

    public bool IsParticipantOnline(string userId)
    {
        return ParticipantOnlineStatus.TryGetValue(userId, out var isOnline) && isOnline;
    }
}
