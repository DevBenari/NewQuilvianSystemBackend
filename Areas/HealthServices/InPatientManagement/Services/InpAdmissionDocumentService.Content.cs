using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Isi khas per jenis dokumen admisi: penerapan isian saat simpan (validation 15.3) dan
    /// pemeriksaan kelengkapan saat kunci (validation 15.4).
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description><b>Serah Terima Pasien Baru</b> (<c>BE-RWI-199</c>): butir dibekukan dari master
    /// saat simpan pertama; setiap butir wajib dipilih Sudah/Belum, dan Belum wajib berketerangan.</description></item>
    /// <item><description><b>Permintaan Privasi</b> (<c>BE-RWI-194</c>, jenis pertama): kerabat dan permintaan
    /// khusus masing-masing paling banyak 3 baris, satu nama satu baris.</description></item>
    /// <item><description><b>Nilai Kepercayaan</b> (<c>BE-RWI-200</c>): 1–5 butir; nama, jenis kelamin,
    /// hubungan, dan alamat penanda tangan wajib.</description></item>
    /// <item><description><b>Selisih Biaya</b> (<c>BE-RWI-201</c>): subjek berkode; nama, alamat, tipe dan
    /// nomor ID deklarer wajib.</description></item>
    /// <item><description><b>Pelunasan Deposit</b> (<c>BE-RWI-202</c>): jatuh tempo berbatas kebijakan;
    /// angka hanya dari Billing.</description></item>
    /// </list>
    ///
    /// <para>
    /// Baris anak konsep (kerabat, butir keyakinan) disunting di tempat menurut nomor barisnya; baris
    /// yang tidak lagi dikirim dihapus fisik selama dokumen masih <c>Draft</c> — konsep belum
    /// ditandatangani dan belum menjadi bukti, sedangkan setiap perubahan tercatat di logger.
    /// </para>
    /// </remarks>
    public sealed partial class InpAdmissionDocumentService
    {
        private const int PartyNameMax = 150;
        private const int PartyRelationshipTextMax = 100;
        private const int PartyAddressMax = 500;
        private const int PartyOccupationMax = 100;
        private const int PartyIdentityNumberMax = 50;
        private const int MobilePhoneMaxDigits = 13;
        private const int OfficePhoneMaxDigits = 20;

        private async Task<InpAdmissionResult<AdmissionDocumentResponse>?> ApplyContentAsync(
            InpAdmissionDocument document,
            AdmissionDocumentContentInput input,
            bool isCreate,
            IReadOnlyList<InpAdmissionHandoverLine>? handoverLines,
            InpEpisodeDepositSummaryDto? depositSummary,
            TimeZoneInfo timeZone,
            DateTime today,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var city = InpAdmissionText.Clean(input.SigningCity);

            if (city?.Length > 100)
            {
                return Bad("Kota maksimal 100 karakter.");
            }

            // Saat dibuat, kota kosong memakai bawaan pengaturan; sesudahnya isi formulir yang berlaku.
            if (!isCreate || city != null)
            {
                document.SigningCity = city;
            }

            if (input.StatementDate.HasValue)
            {
                document.StatementDate = AsDate(input.StatementDate.Value);
            }
            else if (!isCreate)
            {
                document.StatementDate = null;
            }

            var note = InpAdmissionText.Clean(input.Note);

            if (note?.Length > 1000)
            {
                return Bad("Catatan maksimal 1000 karakter.");
            }

            document.Note = note;

            if (InpAdmissionDocumentRules.HasParty(document.DocumentType))
            {
                var partyFailure = await ApplyPartyAsync(document, input.Party, actorUserId, now, cancellationToken);

                if (partyFailure != null)
                {
                    return partyFailure;
                }
            }

            return document.DocumentType switch
            {
                InpAdmissionDocumentType.NewPatientHandover =>
                    ApplyHandover(document, input.HandoverItems, isCreate, handoverLines, actorUserId, now),
                InpAdmissionDocumentType.PrivacyRequest =>
                    ApplyPrivacy(document, input.Privacy, actorUserId, now),
                InpAdmissionDocumentType.BeliefValues =>
                    ApplyBelief(document, input.BeliefItems, actorUserId, now),
                InpAdmissionDocumentType.CostDifferenceStatement =>
                    ApplyCostDifference(document, input.CostDifference, actorUserId, now),
                InpAdmissionDocumentType.DepositSettlementStatement =>
                    ApplyDeposit(document, input.Deposit, isCreate, depositSummary, timeZone, today, actorUserId, now),
                _ => null
            };
        }

        // ---------------------------------------------------------------------
        // Pihak penanda tangan / deklarer
        // ---------------------------------------------------------------------

        private async Task<InpAdmissionResult<AdmissionDocumentResponse>?> ApplyPartyAsync(
            InpAdmissionDocument document,
            AdmissionPartyInput? input,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (input == null)
            {
                if (document.Party != null)
                {
                    _dbContext.Remove(document.Party);
                    document.Party = null;
                }

                return null;
            }

            var resolved = await ResolvePartyAsync(document.PatientId, input, cancellationToken);

            if (!resolved.IsSuccess)
            {
                return resolved.AsFailure<AdmissionDocumentResponse>();
            }

            var values = resolved.Data!;
            var party = document.Party;

            if (party == null)
            {
                party = new InpAdmissionDocumentParty
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                document.Party = party;
            }
            else
            {
                party.UpdateDateTime = now;
                party.UpdateBy = actorUserId;
            }

            party.SourceType = values.SourceType;
            party.SourceRecordId = values.SourceRecordId;
            party.FullName = values.FullName ?? string.Empty;
            party.Relationship = values.Relationship;
            party.RelationshipText = values.RelationshipText;
            party.Address = values.Address;
            party.BirthDate = values.BirthDate;
            party.Gender = values.Gender;
            party.Occupation = values.Occupation;
            party.IdentityType = values.IdentityType;
            party.IdentityNumber = values.IdentityNumber;
            party.MobilePhone = values.MobilePhone;
            party.OfficePhone = values.OfficePhone;

            return null;
        }

        /// <summary>
        /// Saat dikunci, pihak bersumber pasien, relasi, atau kontak darurat dibaca ulang dari
        /// service pemiliknya (<c>BE-RWI-202</c> kriteria 7), supaya salinan beku memakai data terkini.
        /// </summary>
        private async Task<InpAdmissionResult<AdmissionDocumentResponse>?> RefreshPartyFromSourceAsync(
            InpAdmissionDocument document,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var party = document.Party;

            if (party == null || party.SourceType == InpAdmissionPartySource.Manual)
            {
                return null;
            }

            return await ApplyPartyAsync(document, new AdmissionPartyInput
            {
                SourceType = party.SourceType,
                SourceRecordId = party.SourceRecordId,
                FullName = party.FullName,
                Relationship = party.Relationship,
                RelationshipText = party.RelationshipText,
                Address = party.Address,
                BirthDate = party.BirthDate,
                Gender = party.Gender,
                Occupation = party.Occupation,
                IdentityType = party.IdentityType,
                IdentityNumber = party.IdentityNumber,
                MobilePhone = party.MobilePhone,
                OfficePhone = party.OfficePhone
            }, actorUserId, now, cancellationToken);
        }

        /// <summary>
        /// Menetapkan isi pihak. Selain sumber <c>Manual</c>, nama, alamat, dan telepon dibaca ulang
        /// dari service pemiliknya dan isian klien diabaikan (API 12.3 <c>AdmissionPartyInput</c>);
        /// relasi hanya dicocokkan lewat jenis terstruktur, kontak darurat tidak pernah dicocokkan teks
        /// (<c>RWI-DEC-252</c>).
        /// </summary>
        private async Task<InpAdmissionResult<PartyValues>> ResolvePartyAsync(
            Guid patientId,
            AdmissionPartyInput input,
            CancellationToken cancellationToken)
        {
            var source = input.SourceType ?? InpAdmissionPartySource.Manual;

            if (!Enum.IsDefined(source))
            {
                return BadOf<PartyValues>("Sumber data penanda tangan tidak dikenal.");
            }

            if (input.Relationship.HasValue && !Enum.IsDefined(input.Relationship.Value))
            {
                return BadOf<PartyValues>("Hubungan penanda tangan tidak dikenal.");
            }

            if (input.Gender.HasValue && !Enum.IsDefined(input.Gender.Value))
            {
                return BadOf<PartyValues>("Jenis kelamin tidak dikenal.");
            }

            if (input.IdentityType.HasValue && !Enum.IsDefined(input.IdentityType.Value))
            {
                return BadOf<PartyValues>("Tipe ID tidak dikenal.");
            }

            var mobile = InpAdmissionText.NormalizePhone(input.MobilePhone);
            var office = InpAdmissionText.NormalizePhone(input.OfficePhone);

            if (source == InpAdmissionPartySource.Manual && !InpAdmissionText.IsValidPhone(mobile, MobilePhoneMaxDigits))
            {
                return BadOf<PartyValues>(mobile!.All(char.IsDigit)
                    ? "Nomor telepon maksimal 13 digit."
                    : "Nomor telepon hanya boleh berisi angka.");
            }

            if (!InpAdmissionText.IsValidPhone(office, OfficePhoneMaxDigits))
            {
                return BadOf<PartyValues>(office!.All(char.IsDigit)
                    ? "Nomor telepon kantor maksimal 20 digit."
                    : "Nomor telepon kantor hanya boleh berisi angka.");
            }

            var values = new PartyValues
            {
                SourceType = source,
                FullName = InpAdmissionText.Clean(input.FullName),
                Relationship = input.Relationship,
                RelationshipText = InpAdmissionText.Clean(input.RelationshipText),
                Address = InpAdmissionText.Clean(input.Address),
                BirthDate = input.BirthDate.HasValue ? AsDate(input.BirthDate.Value) : null,
                Gender = input.Gender,
                Occupation = InpAdmissionText.Clean(input.Occupation),
                IdentityType = input.IdentityType,
                IdentityNumber = InpAdmissionText.Clean(input.IdentityNumber),
                MobilePhone = mobile,
                OfficePhone = office
            };

            switch (source)
            {
                case InpAdmissionPartySource.Patient:
                    var identity = await _sourceReader.GetPatientIdentityAsync(patientId, cancellationToken);

                    if (!identity.IsAvailable)
                    {
                        return InpAdmissionResult<PartyValues>.Fail(
                            StatusCodes.Status422UnprocessableEntity,
                            "Data pasien tidak dapat dimuat. Pilih Manual atau coba lagi.",
                            InpAdmissionCodes.PartySourceNotFound);
                    }

                    var patient = identity.Value!;
                    values.SourceRecordId = null;
                    values.FullName = Truncate(patient.FullName, PartyNameMax);
                    values.Address = Truncate(InpAdmissionSnapshotBuilder.ComposeAddress(patient), PartyAddressMax);
                    values.MobilePhone = ValidPhoneOrNull(patient.PhoneNumber, MobilePhoneMaxDigits);
                    values.Relationship = InpAdmissionPartyRelationship.Self;
                    values.BirthDate ??= patient.BirthDate.HasValue ? AsDate(patient.BirthDate.Value) : null;
                    values.Gender ??= patient.Gender;
                    values.IdentityType ??= InpAdmissionPrefillService.MapIdentityType(patient.IdentityType);
                    values.IdentityNumber ??= Truncate(patient.IdentityNumber, PartyIdentityNumberMax);
                    break;

                case InpAdmissionPartySource.PatientRelationship:
                case InpAdmissionPartySource.EmergencyContact:
                    if (!input.SourceRecordId.HasValue || input.SourceRecordId.Value == Guid.Empty)
                    {
                        return BadOf<PartyValues>("Pilih data wali atau kontak darurat dari daftar.");
                    }

                    var candidates = await _sourceReader.GetPartyCandidatesAsync(patientId, cancellationToken);

                    if (!candidates.IsAvailable)
                    {
                        return InpAdmissionResult<PartyValues>.Fail(
                            StatusCodes.Status422UnprocessableEntity,
                            "Data wali atau kontak darurat pasien tidak dapat dibaca. Pilih Manual atau coba lagi.",
                            InpAdmissionCodes.PartySourceNotFound);
                    }

                    var expectedSource = source == InpAdmissionPartySource.PatientRelationship
                        ? PatientPartyCandidateSource.PatientRelationship
                        : PatientPartyCandidateSource.EmergencyContact;

                    var candidate = candidates.Value!.FirstOrDefault(x =>
                        x.SourceRecordId == input.SourceRecordId.Value && x.Source == expectedSource);

                    // VAL-RWA-18: hanya relasi atau kontak darurat aktif milik pasien yang sama.
                    if (candidate == null)
                    {
                        return InpAdmissionResult<PartyValues>.Fail(
                            StatusCodes.Status422UnprocessableEntity,
                            "Data wali atau kontak darurat yang dipilih tidak ditemukan pada data pasien.",
                            InpAdmissionCodes.PartySourceNotFound);
                    }

                    values.SourceRecordId = candidate.SourceRecordId;
                    values.FullName = Truncate(candidate.Name, PartyNameMax);
                    values.Address = Truncate(candidate.Address, PartyAddressMax);
                    values.MobilePhone = ValidPhoneOrNull(candidate.PhoneNumber, MobilePhoneMaxDigits);

                    if (expectedSource == PatientPartyCandidateSource.PatientRelationship)
                    {
                        var (relationship, relationshipText) = MapRelationship(candidate.RelationshipType);
                        values.Relationship = relationship;
                        values.RelationshipText = relationshipText;
                    }
                    else
                    {
                        values.Relationship = input.Relationship ?? InpAdmissionPartyRelationship.Other;
                        values.RelationshipText = Truncate(candidate.RelationshipText, PartyRelationshipTextMax) ?? values.RelationshipText;
                    }

                    break;
            }

            if (values.FullName?.Length > PartyNameMax)
            {
                return BadOf<PartyValues>("Nama maksimal 150 karakter.");
            }

            if (values.RelationshipText?.Length > PartyRelationshipTextMax)
            {
                return BadOf<PartyValues>("Keterangan hubungan maksimal 100 karakter.");
            }

            if (values.Address?.Length > PartyAddressMax)
            {
                return BadOf<PartyValues>("Alamat maksimal 500 karakter.");
            }

            if (values.Occupation?.Length > PartyOccupationMax)
            {
                return BadOf<PartyValues>("Pekerjaan maksimal 100 karakter.");
            }

            if (values.IdentityNumber?.Length > PartyIdentityNumberMax)
            {
                return BadOf<PartyValues>("No. ID maksimal 50 karakter.");
            }

            if (values.BirthDate.HasValue && values.BirthDate.Value.Date > DateTime.UtcNow.Date.AddDays(1))
            {
                return BadOf<PartyValues>("Tanggal lahir tidak boleh di masa depan.");
            }

            return InpAdmissionResult<PartyValues>.Ok(values, string.Empty);
        }

        private static (InpAdmissionPartyRelationship Relationship, string? Text) MapRelationship(PatientRelationshipType? type)
            => type switch
            {
                PatientRelationshipType.Spouse => (InpAdmissionPartyRelationship.Spouse, null),
                PatientRelationshipType.Child => (InpAdmissionPartyRelationship.Child, null),
                PatientRelationshipType.Mother => (InpAdmissionPartyRelationship.Parent, "ibu"),
                PatientRelationshipType.Father => (InpAdmissionPartyRelationship.Parent, "ayah"),
                PatientRelationshipType.Sibling => (InpAdmissionPartyRelationship.Sibling, null),
                PatientRelationshipType.Guardian => (InpAdmissionPartyRelationship.Guardian, null),
                PatientRelationshipType.GrandParent => (InpAdmissionPartyRelationship.Other, "kakek/nenek"),
                PatientRelationshipType.ResponsiblePerson => (InpAdmissionPartyRelationship.Other, "penanggung jawab"),
                PatientRelationshipType.EmergencyContact => (InpAdmissionPartyRelationship.Other, "kontak darurat"),
                _ => (InpAdmissionPartyRelationship.Other, "lainnya")
            };

        // ---------------------------------------------------------------------
        // Isi per jenis
        // ---------------------------------------------------------------------

        private InpAdmissionResult<AdmissionDocumentResponse>? ApplyHandover(
            InpAdmissionDocument document,
            List<AdmissionHandoverItemInput>? items,
            bool isCreate,
            IReadOnlyList<InpAdmissionHandoverLine>? lines,
            Guid actorUserId,
            DateTime now)
        {
            var inputs = items ?? new List<AdmissionHandoverItemInput>();

            foreach (var input in inputs)
            {
                if (input.Choice.HasValue && !Enum.IsDefined(input.Choice.Value))
                {
                    return Bad("Pilihan butir serah terima tidak dikenal.");
                }

                if (InpAdmissionText.Clean(input.Note)?.Length > 500)
                {
                    return Bad("Keterangan butir serah terima maksimal 500 karakter.");
                }
            }

            if (isCreate)
            {
                // RWI-DEC-241 butir 4: nama dan kode butir dibekukan saat simpan pertama.
                var frozen = lines ?? Array.Empty<InpAdmissionHandoverLine>();
                var known = frozen.Select(x => x.ClearanceItemId).ToHashSet();

                if (inputs.Any(x => !known.Contains(x.ClearanceItemId)))
                {
                    return Bad("Butir serah terima tidak dikenal pada dokumen ini.");
                }

                foreach (var line in frozen)
                {
                    var input = inputs.LastOrDefault(x => x.ClearanceItemId == line.ClearanceItemId);

                    document.HandoverItems.Add(new InpAdmissionHandoverItem
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = document.Id,
                        ClearanceItemId = line.ClearanceItemId,
                        LineNo = line.LineNo,
                        ItemNumberSnapshot = line.ItemNumber,
                        ParentItemNumberSnapshot = line.ParentItemNumber,
                        ItemCodeSnapshot = line.Code,
                        ItemNameSnapshot = line.Name,
                        Choice = input?.Choice,
                        Note = InpAdmissionText.Clean(input?.Note),
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });
                }

                return null;
            }

            foreach (var input in inputs)
            {
                // VAL-RWA-17: hanya butir yang dibekukan di dokumen itu.
                var item = document.HandoverItems.FirstOrDefault(x => x.ClearanceItemId == input.ClearanceItemId && !x.IsDelete);

                if (item == null)
                {
                    return Bad("Butir serah terima tidak dikenal pada dokumen ini.");
                }

                item.Choice = input.Choice;
                item.Note = InpAdmissionText.Clean(input.Note);
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            return null;
        }

        private InpAdmissionResult<AdmissionDocumentResponse>? ApplyPrivacy(
            InpAdmissionDocument document,
            AdmissionPrivacyInput? input,
            Guid actorUserId,
            DateTime now)
        {
            input ??= new AdmissionPrivacyInput();

            var visitors = CleanLines(input.AllowedVisitors);
            var requests = CleanLines(input.SpecialRequests);

            // VAL-RWA-15 (G-40): tiga baris per jenis, 1–200 karakter per baris.
            if (visitors.Count > 3)
            {
                return Bad("Paling banyak 3 kerabat.");
            }

            if (requests.Count > 3)
            {
                return Bad("Paling banyak 3 permintaan khusus.");
            }

            if (visitors.Any(x => x.Length > 200))
            {
                return Bad("Satu baris kerabat maksimal 200 karakter.");
            }

            if (requests.Any(x => x.Length > 200))
            {
                return Bad("Satu baris permintaan khusus maksimal 200 karakter.");
            }

            if (document.PrivacyRequest == null)
            {
                document.PrivacyRequest = new InpAdmissionPrivacyRequest
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
            }
            else
            {
                document.PrivacyRequest.UpdateDateTime = now;
                document.PrivacyRequest.UpdateBy = actorUserId;
            }

            document.PrivacyRequest.IsTransportPrivacyRequested = input.IsTransportPrivacyRequested;

            SyncPrivacyEntries(document, InpAdmissionPrivacyEntryType.AllowedVisitor, visitors, actorUserId, now);
            SyncPrivacyEntries(document, InpAdmissionPrivacyEntryType.SpecialServiceRequest, requests, actorUserId, now);

            return null;
        }

        private void SyncPrivacyEntries(
            InpAdmissionDocument document,
            InpAdmissionPrivacyEntryType entryType,
            IReadOnlyList<string> texts,
            Guid actorUserId,
            DateTime now)
        {
            for (var index = 0; index < texts.Count; index++)
            {
                var lineNo = index + 1;
                var existing = document.PrivacyEntries.FirstOrDefault(x => x.EntryType == entryType && x.LineNo == lineNo);

                if (existing != null)
                {
                    existing.Text = texts[index];
                    existing.UpdateDateTime = now;
                    existing.UpdateBy = actorUserId;
                    continue;
                }

                document.PrivacyEntries.Add(new InpAdmissionPrivacyEntry
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    EntryType = entryType,
                    LineNo = lineNo,
                    Text = texts[index],
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            foreach (var extra in document.PrivacyEntries.Where(x => x.EntryType == entryType && x.LineNo > texts.Count).ToList())
            {
                document.PrivacyEntries.Remove(extra);
                _dbContext.Remove(extra);
            }
        }

        private InpAdmissionResult<AdmissionDocumentResponse>? ApplyBelief(
            InpAdmissionDocument document,
            List<string>? items,
            Guid actorUserId,
            DateTime now)
        {
            var texts = CleanLines(items);

            // VAL-RWA-16 (RWI-DEC-242): paling banyak lima butir.
            if (texts.Count > 5)
            {
                return Bad("Maksimal 5 butir.");
            }

            if (texts.Any(x => x.Length > 500))
            {
                return Bad("Satu butir maksimal 500 karakter.");
            }

            for (var index = 0; index < texts.Count; index++)
            {
                var itemNo = index + 1;
                var existing = document.BeliefItems.FirstOrDefault(x => x.ItemNo == itemNo);

                if (existing != null)
                {
                    existing.Text = texts[index];
                    existing.UpdateDateTime = now;
                    existing.UpdateBy = actorUserId;
                    continue;
                }

                document.BeliefItems.Add(new InpAdmissionBeliefItem
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    ItemNo = itemNo,
                    Text = texts[index],
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            foreach (var extra in document.BeliefItems.Where(x => x.ItemNo > texts.Count).ToList())
            {
                document.BeliefItems.Remove(extra);
                _dbContext.Remove(extra);
            }

            return null;
        }

        private InpAdmissionResult<AdmissionDocumentResponse>? ApplyCostDifference(
            InpAdmissionDocument document,
            AdmissionCostDifferenceInput? input,
            Guid actorUserId,
            DateTime now)
        {
            if (input?.Subject == null)
            {
                if (document.CostDifferenceStatement != null)
                {
                    _dbContext.Remove(document.CostDifferenceStatement);
                    document.CostDifferenceStatement = null;
                }

                return null;
            }

            var subject = input.Subject.Value;

            if (!Enum.IsDefined(subject))
            {
                return Bad("Subjek pernyataan tidak dikenal.");
            }

            var otherText = InpAdmissionText.Clean(input.SubjectOtherText);

            if (otherText?.Length > 100)
            {
                return Bad("Keterangan subjek maksimal 100 karakter.");
            }

            if (document.CostDifferenceStatement == null)
            {
                document.CostDifferenceStatement = new InpAdmissionCostDifferenceStatement
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
            }
            else
            {
                document.CostDifferenceStatement.UpdateDateTime = now;
                document.CostDifferenceStatement.UpdateBy = actorUserId;
            }

            document.CostDifferenceStatement.Subject = subject;

            // Selama konsep, keterangan "saudara kandung lainnya" yang belum diisi disimpan kosong
            // supaya CHECK terpenuhi; kunci menolaknya (VAL-RWA-24).
            document.CostDifferenceStatement.SubjectOtherText = subject == InpCostDifferenceSubject.OtherSibling
                ? otherText ?? string.Empty
                : null;

            return null;
        }

        private InpAdmissionResult<AdmissionDocumentResponse>? ApplyDeposit(
            InpAdmissionDocument document,
            AdmissionDepositInput? input,
            bool isCreate,
            InpEpisodeDepositSummaryDto? depositSummary,
            TimeZoneInfo timeZone,
            DateTime today,
            Guid actorUserId,
            DateTime now)
        {
            if (document.DepositStatement == null)
            {
                document.DepositStatement = new InpAdmissionDepositStatement
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
            }
            else
            {
                document.DepositStatement.UpdateDateTime = now;
                document.DepositStatement.UpdateBy = actorUserId;
            }

            var statementDate = document.StatementDate ?? today;
            int? interval = depositSummary?.FollowUpIntervalDays;

            if (input?.DueDate.HasValue == true)
            {
                var dueDate = input.DueDate!.Value.Date;
                var message = InpDepositDueDateCalculator.Validate(dueDate, statementDate, interval);

                if (message != null)
                {
                    return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity, message, InpAdmissionCodes.DueDateOutOfRange);
                }

                document.DepositStatement.DueAt = InpDepositDueDateCalculator.ToDueAtUtc(dueDate, timeZone);
            }
            else if (isCreate && interval.HasValue)
            {
                // Bawaan: hari kerja berikutnya pukul 11.00, dipotong ke batas kebijakan (RWI-DEC-248, 261).
                document.DepositStatement.DueAt = InpDepositDueDateCalculator.ToDueAtUtc(
                    InpDepositDueDateCalculator.DefaultDueDate(statementDate, interval.Value), timeZone);
            }

            return null;
        }

        // ---------------------------------------------------------------------
        // Kelengkapan saat kunci (validation 15.4)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Seluruh isian yang kurang dalam satu daftar, supaya petugas tidak mengunci berulang kali.
        /// Contoh Serah Terima: "Butir 5 belum dipilih Sudah atau Belum." dan "Butir 11 berstatus Belum
        /// wajib diberi keterangan." ditolak bersamaan.
        /// </summary>
        private static List<InpAdmissionError> ValidateForLock(
            InpAdmissionDocument document,
            InpEpisodeDepositSummaryDto? depositSummary,
            TimeZoneInfo timeZone)
        {
            var errors = new List<InpAdmissionError>();
            var party = document.Party;
            var cityAndDateMissing = string.IsNullOrWhiteSpace(document.SigningCity) || !document.StatementDate.HasValue;

            switch (document.DocumentType)
            {
                case InpAdmissionDocumentType.NewPatientHandover:
                    foreach (var item in document.HandoverItems.Where(x => !x.IsDelete).OrderBy(x => x.LineNo))
                    {
                        var label = item.ItemNumberSnapshot.HasValue
                            ? $"Butir {item.ItemNumberSnapshot.Value}"
                            : $"Sub-butir {ShortItemName(item.ItemNameSnapshot)} pada butir {item.ParentItemNumberSnapshot}";

                        if (!item.Choice.HasValue)
                        {
                            errors.Add(new InpAdmissionError(InpAdmissionCodes.HandoverItemNotChosen,
                                $"{label} belum dipilih Sudah atau Belum."));
                        }
                        else if (item.Choice == InpHandoverItemChoice.NotDone && string.IsNullOrWhiteSpace(item.Note))
                        {
                            errors.Add(new InpAdmissionError(InpAdmissionCodes.HandoverNoteRequired,
                                $"{label} berstatus Belum wajib diberi keterangan."));
                        }
                    }

                    break;

                case InpAdmissionDocumentType.PrivacyRequest:
                    if (string.IsNullOrWhiteSpace(party?.FullName))
                    {
                        errors.Add(new InpAdmissionError(InpAdmissionCodes.PrivacyIncomplete, "Nama penanda tangan wajib diisi."));
                    }

                    if (cityAndDateMissing)
                    {
                        errors.Add(new InpAdmissionError(InpAdmissionCodes.PrivacyIncomplete, "Kota dan tanggal wajib diisi."));
                    }

                    break;

                case InpAdmissionDocumentType.BeliefValues:
                    if (!document.BeliefItems.Any(x => !x.IsDelete && !string.IsNullOrWhiteSpace(x.Text)))
                    {
                        errors.Add(new InpAdmissionError(InpAdmissionCodes.BeliefIncomplete,
                            "Minimal satu hal yang bertentangan wajib diisi."));
                    }

                    AddMissing(errors, InpAdmissionCodes.BeliefIncomplete, string.IsNullOrWhiteSpace(party?.FullName), "Nama penanda tangan wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.BeliefIncomplete,
                        party?.Gender is null or Gender.Unknown or Gender.NotDisclosed, "Jenis kelamin penanda tangan wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.BeliefIncomplete, party?.Relationship == null, "Hubungan penanda tangan wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.BeliefIncomplete, string.IsNullOrWhiteSpace(party?.Address), "Alamat penanda tangan wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.BeliefIncomplete, cityAndDateMissing, "Kota dan tanggal wajib diisi.");
                    break;

                case InpAdmissionDocumentType.CostDifferenceStatement:
                    var statement = document.CostDifferenceStatement;

                    AddMissing(errors, InpAdmissionCodes.CostDifferenceIncomplete, statement == null, "Subjek pernyataan wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.CostDifferenceIncomplete,
                        statement?.Subject == InpCostDifferenceSubject.OtherSibling && string.IsNullOrWhiteSpace(statement.SubjectOtherText),
                        "Keterangan saudara kandung lainnya wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.CostDifferenceIncomplete, string.IsNullOrWhiteSpace(party?.FullName), "Nama deklarer wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.CostDifferenceIncomplete, string.IsNullOrWhiteSpace(party?.Address), "Alamat deklarer wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.CostDifferenceIncomplete, party?.IdentityType == null, "Tipe ID deklarer wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.CostDifferenceIncomplete, string.IsNullOrWhiteSpace(party?.IdentityNumber), "No. ID deklarer wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.CostDifferenceIncomplete, cityAndDateMissing, "Kota dan tanggal wajib diisi.");
                    break;

                case InpAdmissionDocumentType.DepositSettlementStatement:
                    AddMissing(errors, InpAdmissionCodes.DepositIncomplete, string.IsNullOrWhiteSpace(party?.FullName), "Nama yang menyatakan wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.DepositIncomplete, string.IsNullOrWhiteSpace(party?.Address), "Alamat yang menyatakan wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.DepositIncomplete, string.IsNullOrWhiteSpace(party?.MobilePhone), "Telepon yang menyatakan wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.DepositIncomplete, !document.StatementDate.HasValue, "Tanggal surat wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.DepositIncomplete, document.DepositStatement?.DueAt == null, "Jatuh tempo wajib diisi.");
                    AddMissing(errors, InpAdmissionCodes.DepositIncomplete, string.IsNullOrWhiteSpace(document.SigningCity), "Kota wajib diisi.");

                    var dueAt = document.DepositStatement?.DueAt;

                    if (dueAt.HasValue && document.StatementDate.HasValue)
                    {
                        var dueDate = InpAdmissionText.ToLocal(dueAt.Value, timeZone).Date;
                        var message = InpDepositDueDateCalculator.Validate(
                            dueDate, document.StatementDate.Value, depositSummary?.FollowUpIntervalDays);

                        if (message != null)
                        {
                            errors.Add(new InpAdmissionError(InpAdmissionCodes.DueDateOutOfRange, message));
                        }
                    }

                    break;
            }

            return errors;
        }

        private static void AddMissing(List<InpAdmissionError> errors, string code, bool missing, string message)
        {
            if (missing)
            {
                errors.Add(new InpAdmissionError(code, message));
            }
        }

        /// <summary>"HASIL PEMERIKSAAN PENUNJANG — Radiologi" → "Radiologi".</summary>
        private static string ShortItemName(string name)
        {
            var separator = name.LastIndexOf('—');

            return separator >= 0 && separator < name.Length - 1
                ? name[(separator + 1)..].Trim()
                : name.Trim();
        }

        private static List<string> CleanLines(IEnumerable<string?>? lines)
            => (lines ?? Enumerable.Empty<string?>())
                .Select(InpAdmissionText.Clean)
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();

        private static string? Truncate(string? value, int max)
        {
            var clean = InpAdmissionText.Clean(value);

            return clean == null || clean.Length <= max ? clean : clean[..max];
        }

        private static string? ValidPhoneOrNull(string? raw, int maxDigits)
        {
            var normalized = InpAdmissionText.NormalizePhone(raw);

            return InpAdmissionText.IsValidPhone(normalized, maxDigits) ? normalized : null;
        }

        private static InpAdmissionResult<AdmissionDocumentResponse> Bad(string message)
            => InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status400BadRequest, message);

        private static InpAdmissionResult<T> BadOf<T>(string message)
            => InpAdmissionResult<T>.Fail(StatusCodes.Status400BadRequest, message);

        /// <summary>Isi pihak yang sudah ditetapkan dari sumbernya.</summary>
        private sealed class PartyValues
        {
            public InpAdmissionPartySource SourceType { get; set; }

            public Guid? SourceRecordId { get; set; }

            public string? FullName { get; set; }

            public InpAdmissionPartyRelationship? Relationship { get; set; }

            public string? RelationshipText { get; set; }

            public string? Address { get; set; }

            public DateTime? BirthDate { get; set; }

            public Gender? Gender { get; set; }

            public string? Occupation { get; set; }

            public InpAdmissionPartyIdentityType? IdentityType { get; set; }

            public string? IdentityNumber { get; set; }

            public string? MobilePhone { get; set; }

            public string? OfficePhone { get; set; }
        }
    }
}
