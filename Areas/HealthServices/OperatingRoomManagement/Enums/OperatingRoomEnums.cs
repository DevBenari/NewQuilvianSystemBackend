namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

/// <summary>
/// Status kasus operasi. <c>Rejected = 8</c> ditambahkan <c>BE-RWI-174</c> (<c>RWI-DEC-204</c>,
/// <c>RWI-DEC-208</c>): hanya dari <c>Requested</c>, beralasan, dan final (<c>INV-RWF-31</c>).
/// </summary>
public enum OprCaseStatus { Requested = 1, Scheduled = 2, Ready = 3, InProgress = 4, Completed = 5, Postponed = 6, Cancelled = 7, Rejected = 8 }

/// <summary>
/// Jenis layanan bedah kasus (<c>BE-RWI-174</c>, <c>RWI-DEC-175</c>). Berbeda dari
/// <see cref="OprCaseType"/> (Elektif/Darurat). Kasus lama terbaca <c>General</c>.
/// </summary>
public enum OprSurgicalServiceType { General = 1, Obstetric = 2 }

/// <summary>
/// Rencana jenis anestesi saat kasus dipesan (<c>BE-RWI-174</c>, <c>FR-RWF-042</c>). Nilainya
/// <b>usulan</b> desain 12.8 dan disahkan pemilik OK; teknik sesungguhnya tetap di catatan anestesi.
/// </summary>
public enum OprPlannedAnesthesiaType { General = 1, Regional = 2, Local = 3, Sedation = 4 }

/// <summary>
/// Status versi Catatan Pra-Operasi bangsal (<c>BE-RWI-176</c>, kamus data 19.3).
/// </summary>
public enum OprWardPreOpStatus { Draft = 1, Sent = 2, Confirmed = 3, NeedsUpdate = 4, Superseded = 5 }

/// <summary>Sisi gambar tubuh tempat penandaan area operasi (<c>BE-RWI-176</c>, kamus data 19.3).</summary>
public enum OprBodyView { Front = 1, Back = 2, Left = 3, Right = 4 }
public enum OprCaseType { Elective = 1, Emergency = 2 }
public enum OprPriority { Routine = 1, Urgent = 2, Emergency = 3 }
public enum OprCaseOutcome { Completed = 1, StoppedEarly = 2 }
public enum OprTeamRole { PrimarySurgeon = 1, AssistantSurgeon = 2, Anesthesiologist = 3, ScrubNurse = 4, CirculatingNurse = 5, Other = 99 }
public enum OprCredentialCheckStatus { Pending = 1, Valid = 2, Invalid = 3, NotAvailable = 4 }
public enum OprReadinessRole { PrimarySurgeon = 1, Anesthesiologist = 2, Nurse = 3 }
public enum OprChecklistPhase { SignIn = 1, TimeOut = 2, SignOut = 3 }
public enum OprChecklistStatus { Draft = 1, Completed = 2 }
public enum OprRecordStatus { Draft = 1, Final = 2 }
public enum OprMaterialItemType { Consumable = 1, Implant = 2 }
public enum OprMaterialOutcome { Used = 1, Returned = 2, Wasted = 3, Corrected = 4 }
public enum OprRecoveryStatus { Monitoring = 1, ReadyForRelease = 2, Released = 3 }
public enum OprRecoveryDecision { Inpatient = 1, Icu = 2, OtherUnit = 3, Discharged = 4 }
public enum OprHandoverStatus { Draft = 1, Sent = 2, Accepted = 3, Rejected = 4 }
public enum OprDeliveryStatus { Pending = 1, Processing = 2, Accepted = 3, Failed = 4 }
