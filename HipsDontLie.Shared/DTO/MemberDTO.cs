﻿/// <summary>
/// Represents a participant in a session.
/// </summary>
public class MemberDTO {
    /// <summary>
    /// Gets or sets the unique identifier of the participant.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the participant.
    /// </summary>
    public string Username { get; set; }

    public UserGroupStatus GroupStatus { get; set; }
}