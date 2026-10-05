# Laporan Perubahan Backend — `BE-RWI-175`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-175` |
| Judul | Pemesanan ruang bedah dari bangsal |
| Slice | `MVP-1` / `RWF-W3` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-175` |
| Trace | `FR-RWF-040` s.d. `043`; `RWI-DEC-175`, `176`; `INV-RWF-25`; `AC-RWF-040`, `048`; `UAT-RWF-32`, `42`; API 11.2; `VAL-RWF-70` s.d. `73` |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-174` — ✅ |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, diperiksa 1, diubah 1, logika 1, kontrak API 2, database 0, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/{DTOs,Services,Controllers}`, `OperatingRoomCaseService.cs`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` (adapter), memanggil `OperatingRoomManagement` |
| Prefix registry | `Inp` — `ACTIVE`; `Opr` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (adapter, controller, DTO); `TOUCHED LEGACY` (`OperatingRoomCaseService`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001` |
| Wewenang | Source: ya. Database: tidak ada perubahan schema |

---

## 1. Masalah yang diperbaiki

Perawat bangsal tidak dapat memesan ruang bedah: kasus OK hanya dapat dibuat dokter pemohon dari
layar OK, sehingga pesanan dari Rawat Inap dilakukan lewat telepon dan rawan salah tindakan.

## 2. Proses bisnis

1. Perawat atau dokter bangsal membuka tab Bedah Operasi atau Bedah Obgyn pada episode
   `Admitted`, memilih **tepat satu** order tindakan operasi aktif milik kunjungan episode.
2. Isian: tanggal-jam diinginkan (tidak lebih dari 15 menit di masa lalu), rencana anestesi,
   prioritas, jenis kasus, indikasi, sisi operasi opsional, perkiraan durasi, catatan.
3. Dokter operator diambil dari order; dokter pemesan dari dokter pemberi instruksi order (atau
   dokter order); penginput dari akun login. Tab Obgyn selalu `Obstetric`, tab Bedah Operasi selalu
   `General` — isian klien diabaikan (`VAL-RWF-72`).
4. Adapter memanggil `OperatingRoomCaseService.CreateFromWardBookingAsync` dalam proses yang sama;
   transaksi milik OK. Hasil: 201, kasus `Requested`.
5. Contoh: Budi S., Melati 302/2, order "Appendektomi" aktif, `BookingTab = Surgery`,
   `PreferredAt = 2026-10-03T08:00` → kasus baru berstatus Diminta.
6. Jalur tidak normal: order tidak aktif/bukan milik kunjungan → 422 `INP-SRG-001` "Tindakan operasi
   belum dipesan dokter"; episode `DischargePending` → 422 `INP-SRG-002`; order sudah dipakai kasus
   lain → 409 `OPR002`; kunci idempotensi sama dengan isi berbeda → 409 `OPR013`; isi sama → kasus
   yang sama dikembalikan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OperatingRoomCaseService.cs`, `OperatingRoomCaseDtos.cs`, `InpEpisode.cs`, `InpEpisodeStatus.cs`,
`TrxPatientProcedure.cs`, `PatientProcedureStatus.cs`, `InpatientEpisodeController.cs`,
`InpatientBedOccupancyController.cs`, `PermissionRegistryDescriptor.cs`,
`PermissionRegistryValidator.cs`; kontrak 11.2; validasi 14.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientSurgeryBookingDtos.cs` | Baru — `SurgeryBookingRequest`, `SurgeryBookingTabs` |
| `Areas/HealthServices/InPatientManagement/Services/InpSurgeryBookingAdapter.cs` | Baru — validasi episode dan order, susun `CreateOprCaseRequest`, terjemahkan galat OK |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientSurgeryBookingController.cs` | Baru — `POST episodes/{episodeId}/surgery-bookings` |
| `Areas/HealthServices/OperatingRoomManagement/Services/OperatingRoomCaseService.cs` | `CreateFromWardBookingAsync` (tanpa syarat akun = dokter pemohon); catatan bangsal ke histori `Request` dan ke sidik jari |
| `Program.cs` | Registrasi `InpSurgeryBookingAdapter` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru API 11.2. **Delta:** `Note` disimpan sebagai alasan histori `Request` kasus (OK tidak punya kolom catatan) |
| Database | `NOT APPLICABLE` — tidak ada kolom baru |
| Keamanan/Auth | Endpoint memakai `OperatingRoomCase : Create` sesuai kontrak sebagai **endpoint alias** tanpa `[AccessAction]` kedua: kemampuan itu sudah didaftarkan `OperatingRoomCaseController`, dan mendaftarkannya dari modul Rawat Inap akan membuat resource yang sama tercatat di dua modul (ditolak `PermissionRegistryValidator`). Ini penyimpangan sadar dari aturan "argumen 1 = `ControllerName`" demi kontrak yang disetujui |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Surgery Booking

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `api/v1/health-services/inpatient-management/episodes/{episodeId}/surgery-bookings` | Pesan ruang bedah; header `Idempotency-Key` | `OperatingRoomCase : Create` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight dan kesesuaian | Adapter tanpa transaksi sendiri; controller tanpa `DbContext` | `PASS` | Review source |
| Review registry permission | Kunci `OperatingRoomCase|Create` sudah dideklarasikan; tidak ada resource ganda antar modul | `PASS` | `PermissionRegistryDescriptor.BuildCore` |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Verifikasi API dan proses bisnis dengan order nyata | Belum dijalankan | `NOT RUN` | Butuh build dan database |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Contoh Budi S. "Appendektomi" → 201, kasus `Requested` | Terpenuhi (source) | Controller menjawab 201 dengan `OprCaseDetailResponse` |
| 2. Tanpa order aktif → 422 `INP-SRG-001` | Terpenuhi (source) | `NoActiveOrderCode` |
| 3. Episode `DischargePending` → 422 `INP-SRG-002` | Terpenuhi (source) | `EpisodeNotAdmittedCode` |
| 4. Tab Obgyn → `Obstetric`, tidak dapat diubah dari tab itu | Terpenuhi (source) | `Validate` memetakan tab, isian klien tidak dibaca |
| 5. Kunci sama + isi berbeda → 409; isi sama → tidak dobel | Terpenuhi (source) | Kunci `INP-SRG:`; sidik jari termasuk catatan |
| DoD build tanpa error | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Order "aktif" ditafsirkan `Planned` atau `Ordered`, aktif, bukan dihapus/batal, `IsSurgeryRelated` |
| Masalah yang diketahui | Order yang `Id`-nya tidak ada sama sekali → 404; yang ada tetapi tidak layak → 422 |
| Risiko tersisa | Dokter order nonaktif → 400 dari validasi OK |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `??` `InpatientSurgeryBookingController.cs`, `InpatientSurgeryBookingDtos.cs`, `InpSurgeryBookingAdapter.cs`; `M` `OperatingRoomCaseService.cs`, `Program.cs` |
| Langkah berikutnya | Build oleh pemilik; uji dengan order tindakan nyata |
