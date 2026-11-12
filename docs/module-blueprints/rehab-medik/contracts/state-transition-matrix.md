# Rehab Medik — State Transition Matrix

| Field | Nilai |
|---|---|
| Blueprint | `RHB-BP-001` r1 |
| `last_changed_in` | `v1` — status `draft` |
| Owner | Sukma Giri Pratama — `approved_by` / `approved_at`: belum |
| Input | `00-interview-decisions.md` r1, `02-backend-architecture.md` r1 |
| Traceability | RHB-DEC-005, 007, 009, 011, 013, 021; RHB-INV-02..06; FEAT-001..003, 007..010, 012..018, 024..029, 032 |

Setiap perpindahan status dicatat di `RhbStatusHistory` (pelaku, waktu, alasan). Status yang
tidak tercantum sebagai "Dari" untuk sebuah tindakan berarti tindakan itu **ditolak** dengan
kode 409 dan kode `RHB-VAL-020` ("Tindakan tidak dapat dilakukan pada status saat ini").

Kolom *Siapa yang boleh* menyebut butir hak akses. Pemegangnya diatur admin di Akses Role.
Pemeriksaan kepemilikan data (penulis, profesi) dijaga service.

## 1. Permintaan (`RhbRequest`)

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | `create` | `Draft` | `RehabRequest : Create` | Pasien dipilih | 400/422 |
| `Draft`, `NeedsClarification` | `update` | (tetap) | `RehabRequest : Update` | `expectedVersion` cocok | 409 |
| `Draft`, `NeedsClarification` | `submit` | `Submitted` | `RehabRequest : Update` | Isian wajib lengkap (V-REQ-01..06). Bila ada duplikasi, wajib alasan (V-REQ-07). | 422 / 409 `RHB-VAL-007` beserta daftar kandidat duplikasi |
| `Submitted` | `return` (administratif) | `NeedsClarification` | `RehabRequest : Return` | Alasan wajib | 422 |
| `Submitted` | `request-clarification` (klinis) | `NeedsClarification` | `RehabRequest : Disposition` | Alasan wajib | 422 |
| `Submitted` | `accept` | `Accepted` + episode baru `AwaitingMedicalAssessment` | `RehabRequest : Disposition` | `ResponsibleDoctorId` dokter aktif | 422 |
| `Submitted` | `redirect` | `Redirected` | `RehabRequest : Disposition` | Alasan + tujuan pengalihan | 422 |
| `Submitted` | `reject` | `Rejected` | `RehabRequest : Disposition` | Alasan klinis wajib | 422 |
| `Draft`, `Submitted`, `NeedsClarification` | `cancel` | `Cancelled` | `RehabRequest : Cancel` | Alasan wajib | 422 |

**Transisi tidak sah (contoh):** `Rejected` → `Accepted`, `Accepted` → `cancel`,
`Redirected` → `submit`, dan pemegang `RehabRequest : Return` yang mencoba `reject` (403,
karena butir hak aksesnya berbeda). Pengirim yang permintaannya ditolak membuat permintaan baru.

**Contoh.** Permintaan `RHB-REQ-2026-000014` berstatus `Submitted`. Petugas admisi
mengembalikannya karena surat rujukan BPJS belum terlampir, sehingga statusnya
`NeedsClarification`. Setelah dilengkapi, permintaan di-`submit` ulang menjadi `Submitted`.
dr. Sp.KFR menerimanya, dan terbuka episode `RHB-EPS-2026-000009`.

## 2. Episode (`RhbEpisode`)

| Dari | Tindakan/pemicu | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | permintaan `accept` | `AwaitingMedicalAssessment` | (otomatis) | — | — |
| `AwaitingMedicalAssessment` | asesmen medis ditandatangani | `Planning` | `RehabMedicalAssessment : Sign` | Minimal satu disiplin diresepkan | 422 `RHB-VAL-030` |
| `Planning` | program pertama diaktifkan | `Active` | `RehabProgram : Activate` | §4 | — |
| `AwaitingMedicalAssessment`, `Planning`, `Active` | ringkasan akhir ditandatangani | `Closed` | `RehabDischargeSummary : Sign` | Tidak ada dokumen episode berstatus `Draft` dan tidak ada sesi `InProgress` (V-DIS-02) | 409 `RHB-VAL-060` beserta daftar dokumen yang belum final |
| `Closed` | apa pun | — | — | Episode tertutup tidak dibuka lagi. Layanan baru = permintaan baru. | 409 `RHB-VAL-020` |

Penutupan dari `AwaitingMedicalAssessment` sah untuk alasan `Deceased`, `PatientRefused`,
`Transferred`, `LostToFollowUp`. Contohnya, pasien meninggal sebelum diasesmen.

## 3. Dokumen klinis (asesmen, edukasi, reasesmen, ringkasan)

Status dokumen (`RecordStatus`) berjalan berdampingan dengan status keutuhan Rekam Medis.

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | `create` | `Draft` (keutuhan `Draft`) | butir `Create` resource terkait | Episode belum `Closed` | 409 |
| `Draft` | `update` | `Draft` | butir `Update` + **penulis sendiri** | `EnsureMutableAsync` lolos | 403 bukan penulis; 409 terkunci |
| `Draft` | `sign` | `Final` (keutuhan `Signed`) | butir `Sign` + penulis sendiri | Isian wajib saat tanda tangan lengkap | 422 |
| `Draft` | `cancel` | `Cancelled` (keutuhan `Cancelled`) | butir `Update` + penulis sendiri | Alasan wajib | 422 |
| `Final` | `update` / `cancel` | — | — | Dokumen final tidak berubah | 409 `RHB-VAL-021` "Catatan sudah final, gunakan addendum" |
| `Final` | addendum | `Final` (+addendum) | `ClinicalNoteAddendum : Create` (endpoint RM) | Alasan wajib | ditangani RM |

**Asesmen disiplin** hanya boleh dibuat untuk disiplin yang sudah diresepkan, dan oleh pengguna
yang profesinya sama dengan `RhbDiscipline.ProfessionId` (403 `RHB-VAL-031`).

## 4. Program (`RhbProgram`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | `create` | `Draft` v1 | `RehabProgram : Create` + profesi = disiplin | Disiplin diresepkan | 403/422 |
| `Draft` | `update` | `Draft` | `RehabProgram : Update` + penulis | | 403/409 |
| `Draft` | `activate` | `Active`; versi aktif lama → `Superseded`; episode `Planning` → `Active` | `RehabProgram : Activate` + profesi = disiplin | (a) asesmen medis episode final; (b) asesmen disiplin final; (c) ≥1 intervensi; (d) ≥1 tujuan `Active`; (e) frekuensi, mulai, tanggal evaluasi terisi; (f) tidak ada edukasi `Refused` yang berlaku untuk program ini | 422 `RHB-VAL-040..045` |
| `Active` | `revise` | versi baru `Draft` (lama tetap `Active` sampai versi baru aktif) | `RehabProgram : Create` | Alasan modifikasi wajib | 422 |
| `Active` | `stop` | `Stopped` | `RehabProgram : Stop` | Alasan wajib | 422 |
| `Active` | keputusan reasesmen `Stop` | `Stopped` | `RehabProgressReview : Sign` | — | — |
| `Active` | ringkasan akhir ditandatangani | `Completed` | (otomatis) | — | — |
| `Draft` | `cancel` | `Cancelled` | `RehabProgram : Update` + penulis | | |
| `Superseded`, `Stopped`, `Completed`, `Cancelled` | apa pun | — | — | | 409 `RHB-VAL-020` |

Deviasi sesi yang memengaruhi sesi berikutnya, dan keputusan reasesmen `Modify`, menyetel
`NeedsReview = true` tanpa mengubah status. Sesi tetap boleh berjalan, tetapi layar
menampilkan peringatan.

**Contoh.** Program fisioterapi v1 aktif. Reasesmen 20 Nov memutuskan `Modify`, sehingga v1
mendapat `NeedsReview`. Fisioterapis membuat v2 (`Draft`) lalu mengaktifkannya. Saat itu v1
menjadi `Superseded` dan v2 `Active`. Sesi lama tetap menunjuk v1. Sesi baru menunjuk v2.

## 5. Sesi (`RhbSession`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | `start` | `InProgress` | `RehabSession : Create` + profesi = disiplin program | Program `Active`; encounter terbuka milik pasien; pasien RWI belum pulang fisik; `IdentityVerified` dan `ProgramVerified` = true; kondisi terkini terisi | 409/422 `RHB-VAL-050..054` |
| — | `not-done` | `NotDone` (`Draft`) | `RehabSession : Create` | Alasan tidak terlaksana wajib | 422 |
| `InProgress` | `update` | `InProgress` | `RehabSession : Update` + pelaksana | | 403/409 |
| `InProgress` | `complete` | `Completed` | `RehabSession : Update` + pelaksana | ≥1 intervensi aktual | 422 `RHB-VAL-055` |
| `InProgress` | `stop-early` | `StoppedEarly` | `RehabSession : Update` + pelaksana | Alasan deviasi + tindakan segera wajib | 422 |
| `Completed`, `StoppedEarly`, `NotDone` (`Draft`) | `sign` | `RecordStatus = Final`; handoff billing dimulai bila ada intervensi | `RehabSession : Sign` + pelaksana | Respons terisi bila ada intervensi | 422 |
| `InProgress`, `NotDone` (`Draft`) | `cancel` | `RecordStatus = Cancelled` | `RehabSession : Update` + pelaksana | Alasan wajib, misalnya salah pasien | 422 |
| `InProgress` | `sign` | — | — | Sesi harus diselesaikan dulu | 409 `RHB-VAL-020` |
| `NotDone` | apa pun yang membentuk tagihan | — | — | Tidak pernah tertagih (RHB-INV-06) | — |

## 6. Handoff billing sesi

| Dari | Pemicu | Ke | Siapa |
|---|---|---|---|
| `NotRequired` | sesi ditandatangani dengan ≥1 intervensi | `Pending` | (otomatis) |
| `Pending` | fakta charge terkirim atau terulang (replay) | `Succeeded` | (otomatis) |
| `Pending` | gagal atau hasil tidak pasti | `Failed` / tetap `Pending` | (otomatis) |
| `Failed`, `Pending` | `retry` | `Succeeded` / `Failed` | `RehabBillingHandoff : Retry` |
| `Succeeded` | `retry` | (tetap) | idempoten, tanpa tagihan ganda |
| `NotRequired` | sesi `NotDone`, atau tanpa intervensi | (tetap) | — |

## 7. Reasesmen — efek keputusan saat ditandatangani

| Keputusan | Efek |
|---|---|
| `Continue` | Tidak ada perubahan status |
| `Modify` | Program terkait mendapat `NeedsReview = true` |
| `Stop` | Program terkait → `Stopped` |
| `Refer` | Program terkait → `Stopped`. Tujuan rujukan dicatat untuk ringkasan akhir. |
| `Complete` | Program terkait → `Completed` |

Keputusan `Stop`, `Refer`, dan `Complete` pada reasesmen **medis** (tanpa disiplin) hanya
menandai episode siap ditutup. Episode tetap baru `Closed` lewat ringkasan akhir.

## 8. Tujuan (`RhbGoal`)

| Dari | Tindakan | Ke | Syarat |
|---|---|---|---|
| — | `create` | `Active` | |
| `Active` (belum dirujuk program aktif) | `update` | `Active` | penulis |
| `Active` | `revise` | lama → `Superseded`, baru `Active` | alasan wajib |
| `Active` | reasesmen `GoalReview` | `Achieved` / `PartiallyAchieved` / `NotAchieved` | |
| `Active` | `discontinue` | `Discontinued` | alasan wajib |
| `Active` (dirujuk program aktif) | `update` | ditolak 409 `RHB-VAL-046`, gunakan `revise` | |
