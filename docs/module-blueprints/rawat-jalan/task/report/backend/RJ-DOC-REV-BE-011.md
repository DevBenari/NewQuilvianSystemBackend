# Laporan Perubahan Backend — `RJ-DOC-REV-BE-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-011` |
| Judul | Saklar pemblokir kunjungan aktif (penangguhan sementara) |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `12.1` |
| Trace | `RJ-DOC-DEC-026` (penangguhan), `RJ-DOC-DEC-027` (wewenang); aturan yang ditangguhkan `RJ-DOC-DEC-010` (c), `019`, `022` |
| Kontrak | Bentuk request/response tidak berubah. Perilaku pemblokir `POST /patient-encounters`, `/admin`, `/kiosk` (`RJ-DOC-ENCLIST-001@1.0.0`) mengikuti saklar |
| Task mode | `BACKEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ SELESAI |

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (prefix `Reg`) |
| Keberlakuan | `TOUCHED LEGACY` — `PatientEncounterController.cs`; konfigurasi `appsettings.json` |
| QBE yang berlaku | Conformance Strict pada berkas yang disentuh |
| Hak akses | Tidak berubah. Tanpa hardcode role |
| Migration / DB | Tidak ada |

## 2. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Konstanta `BlockActiveEncounterConfigKey`; `IConfiguration?` sebagai parameter konstruktor opsional terakhir (pemanggil berargumen posisi tidak terdampak); properti `IsActiveEncounterBlockEnabled` (bawaan `false`). Dua titik dibungkus saklar: (1) pemeriksaan di `ValidateCreateRequestAsync`; (2) advisory lock `REG_ENCOUNTER_ACTIVE_*` beserta cek ulang di dalam transaction create. `FindActiveEncounterAsync`, `OutpatientEncounterRules`, dan pesan penolakan tetap utuh |
| `appsettings.json` | `HealthServices:Registration:BlockActiveEncounter: false`. Encoding (UTF-8 BOM, CRLF) dipertahankan |

**Cara menghidupkan kembali aturan:** set `HealthServices:Registration:BlockActiveEncounter` ke
`true` di `appsettings` atau environment (`HealthServices__Registration__BlockActiveEncounter=true`),
lalu restart aplikasi. Tidak perlu perubahan kode.

## 3. Validasi

| Perintah / bukti | Hasil |
| --- | --- |
| `dotnet build … -c Release` | `0 Error(s)`, `244 Warning(s)` (sama dengan baseline; `CS8619` existing bergeser ke baris 2445) |
| QBE Strict `PatientEncounterController.cs` | `VIOLATION 0`, `REVIEW 0`, `PASS` |
| `appsettings.json` | JSON valid; `git diff`: +3 baris |
| `AUTOMATED TEST` | `NOT APPLICABLE` — pola Bank Darah |

### Uji runtime (`QuilvianNewDevSukma`, pasien uji `KSKTEST-RM-07`)

| ID | Saklar | Skenario | Hasil |
| --- | --- | --- | --- |
| Z1 | mati (bawaan) | Daftar kunjungan pertama | `200` (`ENC-RSMMC-00195`) — PASS |
| Z2 | mati | Daftar lagi saat `00195` masih status < 7 | `200` (`00196`), dua kunjungan aktif — PASS |
| Z3 | mati | Daftar Pasien Rawat Jalan menampilkan keduanya | `totalData 2` — PASS |
| Z4 | hidup (`HealthServices__Registration__BlockActiveEncounter=true`) | Daftar lagi | `400` "Pasien masih memiliki kunjungan aktif bernomor ENC-RSMMC-00196 tanggal 02 Okt 2026. …" — PASS |
| Z5 | hidup | Pembersihan `00195`, `00196` lewat `PATCH /outpatient-encounters/{id}/cancel` | `200` ×2; pasien uji tanpa kunjungan aktif — PASS |

Ringkasan: `7/7 PASS`.

## 4. Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| 1. Saklar mati: pasien dengan kunjungan RJ status 0–6 dapat didaftarkan | Terbukti | Z2 |
| 2. Saklar hidup: ditolak `400` dengan pesan `RJ-DOC-REV-BE-007` | Terbukti | Z4 |
| 3. Daftar Pasien Rawat Jalan dan pembatalan tidak berubah | Terbukti | Z3, Z5 |

## 5. Risiko tersisa

1. **Kunjungan ganda kembali mungkin terjadi** selama saklar mati, misalnya karena ketukan ganda di kiosk atau dua loket. Kunjungan menggantung bisa bertambah; pantau lewat kartu Menggantung.
2. **Saklar dibaca per request dari konfigurasi.** Mengubah nilai di `appsettings` memerlukan restart, kecuali konfigurasi di-reload otomatis oleh host.
3. **Server dev pemilik di port 7184 perlu di-restart** supaya memuat perubahan ini.
