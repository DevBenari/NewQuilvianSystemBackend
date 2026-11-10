# Laporan Perubahan Backend — `BE-OPT-051`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-051` |
| Judul | Jasa medis pemeriksaan refraksi |
| Slice | `MVP-4` — `EPIC OPT-09` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-051` |
| Trace | `FR-OPT-091`; `OPT-DEC-042`, `055`; `INT-OPT-08` |
| Contract version | `OPT-INT-v1` `INT-OPT-08` — `approved` sisi Optik |
| Penyetuju modul lain | Medical Fee `OPT-APR-009` ✅ — Yasmin (`OPT-DEC-055`, `OPT-EVD-002`), disampaikan Yoga Aji Pratama: RO dan Sp.M berhak jasa medis |
| Dependency | `BE-OPT-031` ✅ |
| Klasifikasi | `SMALL` — satu sumber baru pada adapter Medical Fee |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/{Services,Models,DTOs}/`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003`, `030`..`050` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalFeeManagement` (pemilik adapter); Optik hanya dibaca |
| Registry | `Mdf` / `ACTIVE`; `Opt` / `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` untuk empat berkas Medical Fee (hanya baris tambahan/ubahan yang dinilai) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-LOG-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Pemeriksaan refraksi sudah ditagih (`BE-OPT-031`), tetapi Medical Fee belum menariknya. Akibatnya RO dan
Sp.M tidak pernah mendapat jasa medis atas pemeriksaan Optik.

---

## 2. Proses bisnis

**Contoh:** Rina (RO) memfinalkan pemeriksaan Ibu Sari (samaran) pada 3 November 2026 pukul 09.20 WIB, ditagih
Rp 75.000; tagihannya final.

1. **Medical Fee menghitung periode November 2026.** Adapter menarik pemeriksaan refraksi yang **final** pada periode itu: satu layanan per pemeriksaan, berapa pun revisinya.
2. **Pelaksana.** Pemeriksaan Rina tampil sebagai layanan dengan penerima **Tenaga Kerja** (profil pegawai Rina). Pemeriksaan dr. Bima (Sp.M) tampil dengan penerima **Dokter** (data dokter dr. Bima). Polanya sama dengan anggota tim Kamar Operasi.
3. **Nilai dasar.** Diambil dari baris tagihan final `OPTICAL` pemeriksaan itu (Rp 75.000). Bila tagihannya belum final atau bertanggal di luar periode, nilainya Rp 0 dan baris tagihan tidak tertaut, sama seperti Laboratorium.
4. **Peran.** Peran pelaksana utama Medical Fee; bila belum diatur, layanan tercatat "peran belum terpetakan". Besar jasa mengikuti aturan bagi hasil Medical Fee, bukan Optik.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MedicalFeeServiceSourceAdapter` (sumber Kamar Operasi, Tindakan, Laboratorium, Adhoc), `AdaptedServiceItem`, `MdfServiceSourceDomains`, `MdfPayeeTypes`, `MdfUnresolvedReasons`, `MedicalFeeCalculationService` (pilihan filter dan ringkasan per domain), `OptRefractionExam`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Services/MedicalFeeServiceSourceAdapter.cs` | **Diperbarui** (+115/−4). `ExtractOpticalExamServicesAsync`; dipanggil dari `ExtractServicesForPeriodAsync`; jumlah Optik di log. **Ubahan kode lama:** saringan tanggal tagihan dari `InvoiceDate.Value.UtcDateTime` ke perbandingan `DateTimeOffset` ber-offset 0 (makna sama: keduanya membandingkan instan UTC) |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Models/MdfServiceFeeDetail.cs` | **Diperbarui.** `MdfServiceSourceDomains.Optical = "OPTICAL"` |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Services/MedicalFeeCalculationService.cs` | **Diperbarui.** `OPTICAL` pada pilihan filter dan ringkasan per domain layanan belum terhitung |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/DTOs/MedicalFeeUnresolvedServiceDtos.cs` | **Diperbarui.** `OPTICAL` pada pilihan filter bawaan |
| `Tests/QuilvianSystemBackend.OpticalTests/Be051MedicalFeeTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 5 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint Medical Fee tidak berubah; nilai `OPTICAL` bertambah pada pilihan domain sumber |
| Kontrak integrasi | `INT-OPT-08` terpenuhi: Medical Fee **menarik** pemeriksaan final dan mencocokkannya dengan baris `OpticalExamCharge` |
| Database | Tanpa migration |
| Keamanan/Auth | Tidak ada izin baru |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru atau berubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 230 lulus, 0 gagal (5 baru) | `PASS` | Keluaran `dotnet test` |
| RO | Satu layanan untuk pemeriksaan dengan 3 revisi; penerima `Workforce` Rina; baris tagihan tertaut; Rp 75.000; tanggal layanan 3 Nov (WIB); peran utama | `PASS` | Uji dengan `ExtractServicesForPeriodAsync` asli |
| Sp.M | Penerima `Doctor` dr. Bima | `PASS` | Uji |
| Draf, batal, di luar periode | Tidak ditarik; tagihan belum final → Rp 0, tanpa tautan | `PASS` | Uji |
| Tagihan final bertanggal di luar periode | Tidak dicocokkan (menguji ubahan saringan tanggal kode lama) | `PASS` | Uji |
| Tanpa peran utama | `ROLE_UNMAPPED` | `PASS` | Uji |
| Uji mutasi: pengenal Sp.M dimatikan | Uji Sp.M gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| Uji mutasi: saringan status final diganti "bukan batal" | Tidak terdeteksi — **setara**: draf tidak punya `FinalizedAt` sehingga tetap tersaring, dan pemeriksaan final tidak dapat dibatalkan | `PASS` (dicatat) | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` atas 4 berkas | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan terpisah) | Terkena 3 aturan; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:**
- Repositori tidak memiliki proyek uji Medical Fee. Regresi sumber lain (Kamar Operasi, Tindakan, Laboratorium, Adhoc) tidak diuji dengan data, hanya terbukti tetap berjalan di harness (tabel kosong).
- Ubahan saringan tanggal belum dicoba di PostgreSQL. Npgsql menerima parameter `DateTimeOffset` ber-offset 0 untuk `timestamptz`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Pemeriksaan final tampil sebagai layanan medis berpelaksana dan cocok dengan baris tagihan | Terpenuhi di harness | Uji RO dan Sp.M |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perlu disampaikan ke pemilik Medical Fee (Yasmin) | (a) Domain sumber baru `OPTICAL`; aturan bagi hasil untuk RO (penerima `Workforce`) dan Sp.M (`Doctor`) perlu diatur di data induk Medical Fee, bila belum maka layanan masuk antrean "belum terhitung". (b) Saringan tanggal tagihan pada adapter diubah bentuknya tanpa mengubah makna |
| Risiko tersisa | Belum dicoba di PostgreSQL dan tanpa uji regresi Medical Fee berdata untuk sumber lain |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Status Git | Tambahan task ini: `M` empat berkas `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/…`, roadmap, traceability; `??` laporan ini. Berkas uji diabaikan Git |
| Langkah berikutnya | `BE-OPT-052` (API empat laporan), lalu `BE-OPT-053` (uji menyeluruh: privasi log, registri izin, alur penuh) |
