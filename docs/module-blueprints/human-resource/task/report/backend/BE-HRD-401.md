# Laporan Perubahan Backend — `BE-HRD-401`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-401` |
| Judul | Integrasi aman dengan Menu 06, 09, dan 12 |
| Slice | `S-FIN-M04` / Lintas Submenu (alias `FIN-HRD-04-BE-015`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-401` |
| Trace | `HRD-DEC-048`, `HRD-DEC-051`, `HRD-DEC-052`; instruksi Menu 04 `FIN-HRD-04-BE-015` |
| Contract version | Kontrak Data Induk dan Workforce Core existing |
| Dependency | `BE-HRD-390` ✅, `BE-HRD-396` ✅, `BE-HRD-399` ✅, `BE-HRD-400` ✅ |
| Klasifikasi | `MEDIUM` — service baru + 3 controller master + 9 controller sub-data |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `MasterData/Workforce/Services/(I)WorkforceLifecycleGuardService.cs` (baru), tiga controller master, sembilan controller `WorkforceCore`, `Program.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji regresi API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Workforce` + `WorkforceCore` |
| Keberlakuan | `NEW CODE` (service penjaga, tanpa entity baru) + `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-SVC-001, QBE-VAL-001, QBE-MOD-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Outcome kartu: pegawai offboard tidak kehilangan histori; perubahan Data Induk tidak mengaktifkan akun purnatugas; profesi tidak memberi kewenangan. Audit menemukan tiga celah nyata:

1. **Histori hilang sesudah pegawai dinonaktifkan.** Sembilan sub-data Menu 04 (alamat, rekening, kontrak, tanggungan, dokumen, pendidikan, kontak darurat, riwayat kepegawaian, keluarga) mensyaratkan profil **aktif**. Begitu HR menonaktifkan pegawai yang pensiun, semuanya membalas `404 Profil tenaga kerja tidak ditemukan` — dokumen dan riwayat purnatugas tidak dapat dibuka.
2. **Akun purnatugas hidup kembali.** `PUT`/`PATCH /status` Data Induk menyalin `IsActive` ke pegawai, profil, dan akun login. Perawat A pensiun 31 Oktober 2026; 5 November petugas mengedit alamatnya dan tidak sengaja menyalakan "Aktif" — akun login A aktif lagi.
3. **Kunci skorsing terbuka.** Worker skorsing (`EMP-W6`) mengunci akun pegawai yang diskors. Edit Data Induk menulis `linkedUser.IsActive = true` dan membuka kunci itu.

## 2. Proses bisnis

1. Sub-data pegawai dapat dibuka selama profil tidak dihapus (aktif maupun nonaktif).
2. Mengaktifkan kembali (nonaktif → aktif) pegawai, dokter, atau pengguna eksternal yang pemberhentiannya **final** (`TrxEmployeeSeparation` berstatus `FinalizedCompleted`/`Completed` atau `CompletedAt` terisi, dan tanggal efektif sudah lewat) → `409 SEPARATED_WORKFORCE_REACTIVATION_BLOCKED`. Penerimaan kembali lewat Rekrutmen dan Onboarding.
3. Saat Data Induk menyimpan status akun login, akun yang tindakan skorsing terakhirnya `Locked` tetap nonaktif; pemulihan oleh worker saat skorsing berakhir.
4. Profesi tidak memberi kewenangan klinis (`BE-HRD-399`); riwayat SK tetap hanya-baca (`BE-HRD-396`); perubahan sensitif lewat Menu 06 dengan target milik profil yang sama (`BE-HRD-390`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Tiga controller master (`PUT`, `PATCH /status`, `DELETE`), sembilan controller sub-data (`WorkforceProfileExistsAsync`), `TrxEmployeeSeparation`, `EmployeeSeparationService`, `ExitClearanceService` (status `FinalizedCompleted`), `HrdSuspensionAccessAction`, worker `ItAccessRevocationSyncWorker`, `HrdOnboardingCaseService` (menonaktifkan profil saat batal bergabung).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MasterData/Workforce/Services/IWorkforceLifecycleGuardService.cs`, `WorkforceLifecycleGuardService.cs` (baru) | `IsSeparationFinalizedAsync`, `HasActiveSuspensionLockAsync` |
| `Program.cs` | Registrasi scoped service penjaga |
| `EmployeeController.cs`, `DoctorController.cs`, `ExternalUserController.cs` | Penjaga reaktivasi pada `PUT` dan `PATCH /status`; akun terkunci skorsing tidak dibuka |
| `WfpAddressController`, `WfpBankAccountController`, `WfpContractHistoryController`, `WfpDependentController`, `WfpDocumentController`, `WfpEducationController`, `WfpEmergencyContactController`, `WfpEmploymentHistoryController`, `WfpFamilyMemberController` | `WorkforceProfileExistsAsync` hanya mensyaratkan profil tidak dihapus |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap. Sub-data profil nonaktif kini `200` (dulu `404`); kode galat baru `SEPARATED_WORKFORCE_REACTIVATION_BLOCKED` (409) |
| Database | Tanpa migration; hanya membaca `TrxEmployeeSeparation` dan `HrdSuspensionAccessAction` |
| Keamanan/Auth | Mencegah akun purnatugas aktif kembali dan kunci skorsing terbuka |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Master Data / Employee

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/corporate/human-resource/master-data/employees/{id}` | Reaktivasi purnatugas ditolak `409` | `Employee : Update` |
| `PATCH` | `…/employees/{id}/status` | Idem | `Employee : Update` |

(Padanan untuk Doctor dan External User.)

#### Corporate / Human Resource / Workforce Core / *

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/{documents,educations,…}` | Dapat dibuka untuk profil nonaktif | Hak masing-masing resource (tidak berubah) |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: dokumen pegawai nonaktif | `200` (dulu `404`) | `PASS` (penelusuran) | `WorkforceProfileExistsAsync` |
| Penelusuran: aktifkan pegawai pensiun final | `409` | `PASS` (penelusuran) | `IsSeparationFinalizedAsync` |
| Penelusuran: edit pegawai diskors | Akun tetap nonaktif | `PASS` (penelusuran) | `HasActiveSuspensionLockAsync` |
| Penelusuran regresi: SK (`HrdPersonnelActionOrderService`) dan clearance tidak memanggil berkas yang diubah selain lewat DbContext | Tidak terdampak | `PASS` (penelusuran) | pencarian pemanggil |
| Uji regresi API lintas menu di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Regresi SK, offboarding, dan kredensial tidak rusak | Terpenuhi (source) | Tidak ada perubahan pada service SK/clearance/kredensial; penjaga hanya membaca; bukti API `NOT RUN` |
| (2) Histori tetap ada setelah offboarding | Terpenuhi | Sub-data dapat dibuka untuk profil nonaktif; riwayat SK tidak dapat disembunyikan (`BE-HRD-396`) |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | (a) Sub-data modul lain (kredensial, pelatihan, kesehatan kerja) masih mensyaratkan profil aktif — di luar Menu 04; (b) `DELETE` Pegawai menghapus (soft delete) seluruh penempatan organisasi — perilaku lama, perlu keputusan pemilik apakah hapus Pegawai masih diizinkan untuk pegawai yang punya riwayat |
| Risiko tersisa | Bukti regresi runtime belum ada |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-402` |
