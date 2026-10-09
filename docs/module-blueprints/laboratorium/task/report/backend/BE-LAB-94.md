# Laporan Perubahan Backend — `BE-LAB-94`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-94` |
| Judul | Pemeriksaan mewarisi CITO saat wadah direncanakan dan diambil ulang, beserta perbaikan data lama |
| Slice | Gelombang `MVP-14a` — CITO dari pemesanan sampai ke pemeriksaan (putaran 28, `LAB-CONFLICT-019`) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6at.1 |
| Trace | `LAB-DEC-226`, `LAB-DEC-227`, `LAB-DEC-228` (+ `LAB-REQ-022` butir 4); `AC-318`..`AC-322` |
| Contract version | `LAB-API-v1` **`r45`** bagian 40 — `approved` 2026-10-09 (`LAB-REQ-022`, *"setuju keenam butir"*) |
| Desain | [`02-backend-architecture.md`](../../../02-backend-architecture.md) bagian 29 |
| Dependency | Nol task pendahulu |
| Klasifikasi | `MEDIUM` — 1 berkas source diubah, 1 migration data baru; nol tabel, kolom, endpoint, DTO, izin |
| Task mode | `BACKEND` |
| Wewenang | Source backend dan **pembuatan** migration data (`LAB-REQ-022` butir 6). **Tanpa** `database update` dan tanpa deploy. Uji SQL di DB dev **dalam transaksi `ROLLBACK`** atas izin pemilik modul 2026-10-09; pesanan uji sungguhan **tidak** diizinkan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e6c0e451` (branch `yoga`) + dokumen putaran 28 di working tree |
| Commit frontend | `8f3d74d18` (branch `YogaV2`) — nol berkas berubah |
| Tanggal | 2026-10-09 |
| Status | ✅ **`SELESAI`** — harness InMemory **18/18**, uji SQL migration **13/13** (transaksi `ROLLBACK`), build 0 error, migration diterapkan ke devYoga (riwayat 296 → 297, 0 baris data). **Cek layar 2026-10-09 (MVP-14c langkah 5, izin pemilik)**: satu pesanan uji `LAB-RSMMC-000024` (superadmin, kunjungan `ENC-RSMMC-00177`, Hemoglobin CITO + Leukosit biasa), *Terima Sampling* oleh analis **Gilang** sampai *Received* (plan `201`, collect `200`, receive `200`; tanpa Layak, tanpa tagihan). Daftar Pasien PK: Kesegeraan *Biasa* → **Memuat CITO**; saringan *Cito* memuatnya; Nota tercetak *Hemoglobin CITO* / *Leukosit Biasa*; Daftar Kerja: Hemoglobin CITO baris 2, Leukosit baris 9; *Hanya Cito* memuat Hemoglobin saja; API: Hemoglobin `Cito` penanda `null`. **Catatan sisa, bukan penahan:** `AC-319` sisi layar (Pantau Keterlambatan Cito) butuh wadah *Layak* — pemilik memilih tidak menerbitkan tagihan uji; bukti harness |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Sumber governance | `AGENTS.md` backend (lapisan operasional di suite Skill terpasang `rules/backend/`); `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — ketiganya terbaca |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix | `Lab` — registry baris 28, status **`ACTIVE`** (sejak 2026-09-02, `LAB-REQ-002`) |
| Keberlakuan | `TOUCHED LEGACY` (`LabSpecimenService.cs`); migration data = `NEW CODE` tanpa entity baru |
| Entity baru | Nol. Record privat `LabExaminationPlan` hanya pembawa nilai di dalam service |
| Pola yang ditiru | Migration tulisan tangan dengan atribut `[DbContext]`/`[Migration]` di berkas utama (`20260911000000_RenameNutritionGzPrefixToGzi.cs`); migration data berpenanda dengan `Down` berpenanda (`20260921032943_BackfillEmergencyDoctorAssignment.cs`) |
| Database | Pembuatan migration: berwenang. Eksekusi: **tidak** — `database update` tidak dijalankan |

---

## 1. Masalah yang diperbaiki

Dokter atau kiosk memilih CITO per pemeriksaan saat memesan, dan pilihan itu tersimpan di `LabOrderedProcedure.Urgency`.
Tetapi `CreateExaminationsAsync` selalu menulis `Urgency = Routine` saat wadah direncanakan, sehingga pasien CITO:

- tidak berlencana CITO di Daftar Pasien Lab;
- tidak didahulukan di Daftar Kerja;
- tidak terpantau di Pantau Keterlambatan Cito;
- tercetak biasa pada label.

Jalur ambil ulang memakai method yang sama, sehingga CITO hasil *Tandai Cito* dokter juga hilang setiap kali pasien
ditusuk ulang.

## 2. Proses bisnis

1. dr. Arif memesan Hemoglobin **CITO** dan Leukosit biasa.
2. Analis merencanakan wadah → Hemoglobin lahir **Cito** (penanda kosong), Leukosit Routine.
3. Hemoglobin naik ke atas Daftar Kerja. Begitu wadah *Layak* dan batas 60 menit lewat, ia muncul di Pantau
   Keterlambatan Cito.
4. dr. Arif menandai Leukosit CITO lewat *Tandai Cito*, lalu sampel ditolak dan diambil ulang. Di wadah pengganti
   **keduanya** tetap Cito, dan penanda Leukosit (dr. Arif, jamnya) ikut tersalin.
5. Bila dr. Arif sempat **mencabut** cito suatu pemeriksaan, pengganti ikut Routine dengan penanda pencabutannya.
   Permintaan awal tidak dibaca ulang.

**Saat rilis**, pekerjaan yang masih berjalan dan terlanjur Routine dipulihkan sekali:

- dari permintaan CITO — penanda tetap kosong;
- dari wadah pengganti yang kehilangan CITO — penanda pemeriksaan lama ikut tersalin.

Setiap pemulihan meninggalkan satu baris riwayat berpenanda `LAB-CONFLICT-019`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs` | Record privat `LabExaminationPlan(Procedure, Urgency, UrgencyMarkedAt, UrgencyMarkedByUserId)`. `CreateExaminationsAsync` menerima daftar rencana dan menulis ketiga ruas kesegeraan darinya. `PlanAsync` memanggil `ResolveOrderedUrgencyAsync` (baru), satu kueri proyeksi `ProcedureId`/`Urgency` atas permintaan pesanan yang bukan `Cancelled` dan bukan terhapus; Cito bila salah satunya Cito; tanpa permintaan → Routine; penanda kosong. `RequestRecollectionAsync` menyusun rencana dari `pemeriksaanLama` yang sudah dimuat — kesegeraan dan penanda disalin, nol kueri tambahan. Urutan, tarif, `MarkOrderedProceduresFulfilledAsync`, dan log tidak berubah |
| `Migrations/20261009100000_BackfillLabExaminationUrgencyFromOrderedProcedure.cs` | **Baru**, data saja. Up: (1) riwayat dan (2) `UPDATE` untuk permintaan CITO (`LAB-DEC-228`); (3) riwayat dan `UPDATE` untuk wadah pengganti, sumber dicari mundur sepanjang `SupersededSpecimenId` lewat CTE rekursif (kedalaman ≤ 20). `Version + 1`, pelaku `Guid.Empty`, `ReasonCode = 'LAB-CONFLICT-019'`. Down: hanya pemeriksaan yang tidak disentuh *Tandai Cito* sesudahnya yang dibalik, beserta riwayat berpenandanya |

**Tidak berubah:** `ApplicationDbContextModelSnapshot.cs`, `LabExaminationService` (`AddAsync` tetap Routine —
`LAB-REQ-022` butir 3; `SetUrgencyAsync`), controller, DTO, izin, frontend.

### 3.2 Kepatuhan

- Nol arsitektur baru, nol repository generik.
- Service tetap memakai `_dbContext` milik service tanpa transaksi baru.
- Migration mengikuti dua preseden yang disebut di preflight.
- `ActorUserId` pada `LabTransitionHistory` **tanpa** FK, sehingga `Guid.Empty` sah. Hal ini diperiksa pada
  konfigurasi dan snapshot.

## 4. Dokumentasi endpoint

#### `[Tags("Health Services / Laboratory Management / Lab Specimen")]`

| Method | Path | Hak akses | Perubahan perilaku (`r45`) |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/laboratory-management/lab-specimens/by-order/{labOrderId}` | `LabSpecimen : Plan` | Pemeriksaan mewarisi `urgency` permintaan; `urgencyMarkedAt`/`urgencyMarkedByUserName` `null` |
| `POST` | `/api/v1/health-services/laboratory-management/lab-specimens/{id}/request-recollection` | `LabSpecimen : Accept` | Pemeriksaan pengganti menyalin `urgency` dan penanda pemeriksaan yang digantikan |

Request, response, kode status, dan pesan galat tidak berubah.

## 5. Verifikasi

### 5.1 Harness InMemory — 18/18

Proyek scratchpad dengan `ProjectReference` ke backend dan `Microsoft.EntityFrameworkCore.InMemory`. Nol tulis ke DB
bersama.

| Kelompok | Hasil |
| --- | --- |
| `AC-318` wadah pertama | Hemoglobin (permintaan Cito) → Cito; Leukosit → Routine; penanda kosong keduanya; permintaan tetap `Fulfilled` dan tertaut; **nol** riwayat `SetUrgency` saat lahir |
| `AC-318` hilir | Daftar kerja `onlyCito` memuat Hemoglobin saja; Hemoglobin di atas Leukosit (`LAB-FE-006`) |
| `AC-319` | Layak 90 menit lalu, batas 60 → `cito-overdue` terlambat 30 menit; pada Layak+30 menit belum muncul |
| `AC-322` | Pesanan tanpa permintaan → Routine; kartu CITO Beranda `citoOrderCount = 1` (rumus dua sumber, tidak ganda) |
| Tambahan | Dua permintaan aktif prosedur sama (satu Cito) → Cito; permintaan Cito yang **dibatalkan** tidak mewariskan |
| `AC-320` ambil ulang | Tiga pemeriksaan pengganti. Hemoglobin Cito (penanda kosong); Leukosit Cito dengan penanda dokter dan waktu **persis**; Glukosa yang dicabut dokter → Routine berpenanda pencabutan, permintaan Cito **tidak** dibaca ulang; permintaan berpindah tautan ke pengganti |

### 5.2 Uji SQL migration — PostgreSQL 15 (devYoga), transaksi `ROLLBACK` — 13/13

SQL diambil dari `UpOperations`/`DownOperations` kelas migration yang sudah dikompilasi, bukan salinan.

**Persiapan di dalam transaksi.**

- Permintaan tiga pesanan dibuat Cito: `000009` positif, `000022` pemeriksaan *Gugur*, `000023` berpenanda dokter.
- Dua rantai salinan wadah: **A→B→C**, dengan A Cito bertanda dan B *Gugur*; serta **A2→B2→C2**, dengan B2 berupa
  pencabutan dokter.

| Pemeriksaan | Hasil |
| --- | --- |
| Langkah 1–2 | `000009` → Cito, penanda kosong, `Version` 0→1, tepat satu riwayat |
| Negatif | `000022` (*Gugur*) dan `000023` (penanda dokter) tidak disentuh; `Version` tetap |
| Langkah 3 | C pulih Cito dengan penanda dokter A — melewati B yang *Gugur*; satu riwayat; B tidak diubah; C2 tetap Routine |
| Riwayat | Total 2 baris: pelaku sistem, `Examination.SetUrgency`, `Routine → Cito`, `Scope = 3` |
| Idempoten | Jalan kedua: riwayat tetap 2, `Version` tetap |
| Down | `000009` kembali Routine dan riwayatnya terhapus; C yang disentuh *Tandai Cito* sesudah migration **dibiarkan** beserta riwayatnya |
| Sesudah `ROLLBACK` | Nol riwayat berpenanda; tiga pemeriksaan dasar persis seperti semula; nol wadah salinan |

### 5.3 Lain-lain

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error, 2 menit 4 detik; server BE/FE dimatikan sebelumnya | `PASS` |
| `dotnet ef migrations list --no-connect --no-build` | Migration baru terdaftar sebagai yang **terbaru** | `PASS` |
| Snapshot model | `git status`: tidak berubah | `PASS` |
| Migrasi otomatis saat start | Nol `Migrate()` di source — menyalakan backend **tidak** menerapkan migration | `PASS` |
| Hitung kering dev (baca-saja) | 0 baris langkah 1–2, 0 baris langkah 3 | `PASS` |
| Regresi HTTP baca-saja ke biner baru (superadmin) | `dashboard/today` `200` (`citoOrderCount` 0); `lab-examinations/by-order` `000023` `200` (Hemoglobin Routine, penanda `null` — tidak berubah); pesanan tak ada `404`; `lab-worklists/pending?onlyCito=true` `200` (`totalData` 1) | `PASS` |
| HTTP/peramban dengan pesanan CITO sungguhan (2026-10-09, izin pemilik) | **Cek layar 2026-10-09 (MVP-14c langkah 5, izin pemilik)**: satu pesanan uji `LAB-RSMMC-000024` (superadmin, kunjungan `ENC-RSMMC-00177`, Hemoglobin CITO + Leukosit biasa), *Terima Sampling* oleh analis **Gilang** sampai *Received* (plan `201`, collect `200`, receive `200`; tanpa Layak, tanpa tagihan). Daftar Pasien PK: Kesegeraan *Biasa* → **Memuat CITO**; saringan *Cito* memuatnya; Nota tercetak *Hemoglobin CITO* / *Leukosit Biasa*; Daftar Kerja: Hemoglobin CITO baris 2, Leukosit baris 9; *Hanya Cito* memuat Hemoglobin saja; API: Hemoglobin `Cito` penanda `null` | `PASS` |
| `database update` | — | `NOT RUN` dalam task — di luar wewenang task |
| **Langkah rilis `MVP-14c` di devYoga** (instruksi pemilik modul 2026-10-09, sesudah task) | `migrations list`: dua tertunda — `20261006150000_AddInvoiceDateAndTermsToFinReceivableInvoiceBatch` (Keuangan, **tidak** disetujui) dan migration ini. Karena itu **bukan** `database update`, melainkan `migrations script <Keuangan> <ini>` (hanya migration Lab), uji kering naik → turun → `ROLLBACK` lolos, lalu naik + `COMMIT`. Riwayat 296 → 297; 0 riwayat berpenanda (hitung kering 0); sidik `md5` `LabExamination` sama sebelum-sesudah; migration Keuangan **tetap tertunda** | `PASS` |

AUTOMATED TEST: PASS (harness 18/18; SQL 13/13). Uji otomatis tinggal di scratchpad sesi (`LAB-RDY-C04`).

## 6. Hitung kering untuk langkah rilis `MVP-14c`

Jalankan **baca-saja** sebelum `database update`, lalu sekali lagi sesudahnya (harus 0).

```sql
-- Langkah 1-2: permintaan CITO yang pemeriksaannya masih Routine tanpa penanda, pekerjaan masih berjalan.
SELECT count(DISTINCT e."Id")
FROM public."LabOrderedProcedure" p
JOIN public."LabExamination" e ON e."Id" = p."FulfilledExaminationId"
JOIN public."LabOrder" o ON o."Id" = e."LabOrderId"
WHERE NOT p."IsDelete" AND p."Urgency" = 2 AND p."OrderedStatus" = 2
  AND NOT e."IsDelete" AND e."Urgency" = 1 AND e."UrgencyMarkedAt" IS NULL AND e."ExaminationStatus" NOT IN (3, 4)
  AND NOT o."IsDelete" AND o."OrderStatus" NOT IN (5, 8);

-- Langkah 3 (perkiraan satu tingkat): pemeriksaan di wadah pengganti yang Routine tanpa penanda,
-- sementara pemeriksaan prosedur sama di wadah yang digantikan Cito. Angka pasti ada pada CTE migration.
SELECT count(*)
FROM public."LabSpecimen" sb
JOIN public."LabExamination" eb ON eb."SpecimenId" = sb."Id" AND NOT eb."IsDelete"
JOIN public."LabExamination" ea ON ea."SpecimenId" = sb."SupersededSpecimenId" AND ea."ProcedureId" = eb."ProcedureId" AND NOT ea."IsDelete"
JOIN public."LabOrder" o ON o."Id" = eb."LabOrderId"
WHERE sb."SupersededSpecimenId" IS NOT NULL AND NOT sb."IsDelete"
  AND eb."Urgency" = 1 AND eb."UrgencyMarkedAt" IS NULL AND eb."ExaminationStatus" NOT IN (3, 4)
  AND ea."Urgency" = 2 AND NOT o."IsDelete" AND o."OrderStatus" NOT IN (5, 8);
```

DevYoga 2026-10-09: **0** dan **0**.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-318` sisi backend | Terpenuhi | 5.1 |
| `AC-318` sisi layar | Terpenuhi 2026-10-09 | Lencana, saringan, Nota, Daftar Kerja — akun analis asli |
| `AC-319` sisi backend | Terpenuhi | 5.1 |
| `AC-320` | Terpenuhi | 5.1 |
| `AC-321` | Terpenuhi | 5.2 (PostgreSQL sungguhan, `ROLLBACK`) |
| `AC-322` | Terpenuhi | 5.1 |
| Tambahan (dua permintaan; tambah manual tetap Routine) | Terpenuhi — `AddAsync` tidak disentuh (diff) | 5.1, 3.1 |
| DoD — build, snapshot tetap, hitung kering tercatat, laporan | Terpenuhi | 5.3, 6 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Temuan T1** | Jalur **wadah dibatalkan lalu direncanakan ulang** juga lewat `PlanAsync`, sehingga — sesuai rancangan yang disetujui — membaca **permintaan**, bukan keadaan terakhir pemeriksaan lama. Bila dokter sempat *Tandai Cito*/mencabut sebelum wadahnya dibatalkan, keputusan itu tidak terbawa ke wadah baru. Tidak diperluas diam-diam; perlu keputusan pemilik modul bila ingin disamakan dengan `LAB-DEC-227` |
| Risiko tersisa | Lama kueri langkah 3 di produksi belum diketahui (CTE rekursif, tanpa index tambahan) — hitung kering langkah rilis memberi gambaran ukurannya |
| Dependency backend | Tidak ada. Frontend tidak perlu dirilis bersama |
| Perubahan sampingan | `NONE` |
| Server lokal | Dimatikan sebelum build, dinyalakan lagi sesudahnya (backend memuat `BE-LAB-94`; migration **tidak** diterapkan) |
| Status Git | Backend: 1 `M` (`LabSpecimenService.cs`), 1 `??` (migration) + dokumen — belum di-commit |
| Task berikutnya | Langkah rilis `MVP-14c` (6at.2): hitung kering → deploy → `database update` atas instruksi → hitung kering ulang 0 → cek layar |
