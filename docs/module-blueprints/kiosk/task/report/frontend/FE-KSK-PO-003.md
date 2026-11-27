# Laporan Perubahan Frontend — `FE-KSK-PO-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-PO-003` |
| Judul | Data Rujukan, Consent, Verifikasi, Kirim, Selesai |
| Slice | `EPIC KSK-PO-05` — halaman publik frontend (`FR-KSK-PO-041`, `042`), gelombang 3, rilis `MVP-1` |
| Roadmap | [online-registration-frontend-roadmap.md](../../../roadmap/online-registration-frontend-roadmap.md) — kartu `FE-KSK-PO-003` |
| Trace | `KSK-DEC-034`, `035`, `041`, `042`, `044`; `KSK-PO-FE-003`; `KSK-DSN-PO-011` |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`), grup *Online Registration Public* `POST /public` (multipart), `GET options/referral-institutions`; validation matrix `KSK-VAL-PO-021` s.d. `034` |
| Wewenang UI | `KSK-PO-FE-001` (langkah sama dengan RJ), `KSK-PO-FE-002` (hanya data tersamar), `KSK-PO-FE-003` (isi email/WhatsApp: kode, tanggal, poli, dokter), `KSK-PO-FE-004` (tata letak `DEV_DISCRETION`) |
| Dependency | `FE-KSK-PO-002` ✅, `BE-KSK-PO-005` ✅ |
| Klasifikasi | `MEDIUM` — kiriman multipart publik dengan captcha, berkas, dan tanda tangan; dua komponen bersama RJ mendapat ekspor/prop opsional |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: view, hook, service, util, konstanta, dan style halaman online; ekspor + prop opsional di `outpatient-referral-step.jsx`; laporan dan baris status di blueprint kiosk (repository backend) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `508c8beb8` (`sukmagpV2`) + perubahan `FE-KSK-PO-001`/`002` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `93f0fa1b` (`sukmagp`) + perubahan `BE-KSK-PO-008`/`010` yang belum di-commit |
| Tanggal | 2026-11-20 |
| Status | ✅ Selesai — AC 5, 6, 14, 15 (tampilan dan kiriman) dan kriteria tambahan 1–3 terbukti di browser dan di data backend |

## 1. Keadaan yang ditemukan di awal

Sesudah `FE-KSK-PO-002`, langkah Data Rujukan, General Consent, Verifikasi, dan Selesai baru berisi penanda "disiapkan pada tahap berikutnya". Ketiga langkah RJ yang direncanakan untuk dipakai ulang diperiksa satu per satu:

| Komponen RJ | Temuan | Akibat bagi halaman publik |
| --- | --- | --- |
| `OutpatientGeneralConsentStep` | Murni: hanya membaca isian form; isian awal "Pasien Sendiri" dibaca dari nama isian RJ `fullName`, `identityNumber`, `phoneNumber`, `address` | Dapat dipakai apa adanya |
| `OutpatientReferralStep` | Memanggil `useSelectResource("referralPartnerInstitutions")` (endpoint petugas) begitu dibuka; punya sumber Internal (Surat Rujukan dokter), tujuan Laboratorium/Radiologi, jam rujukan, dan berkas **opsional** sampai 10 | Request petugas tanpa token → `401` → pengunjung dapat dialihkan ke login; aturan online (eksternal saja, 1–3 berkas wajib, diagnosa dan alasan) tidak terwakili |
| `OutpatientVerificationStep` | Menampilkan nama lengkap, No. RM, NIK, alamat, data HR/Benefit Karyawan, dan "Proses Penyimpanan" kunjungan; tanpa tempat captcha; tombol "Selesaikan Pendaftaran" | Melanggar `KSK-PO-FE-002` untuk pasien lama dan tidak cocok dengan kontrak tombol "Mengirim..." (`KSK-VAL-PO-034`) |

Backend menjawab galat Kirim hanya dengan `message` (tanpa kode aturan), sehingga jenis `409` dibedakan dari kalimatnya yang baku di validation matrix. Enum dikirim sebagai angka (tidak ada `JsonStringEnumConverter`), termasuk `GeneralConsentSignerType` yang di langkah consent RJ disimpan sebagai nama (`"Self"`).

## 2. Proses bisnis dari sisi pengguna

1. **Data Rujukan** (khusus Pasien Rujukan, sesudah Data Kunjungan). Pasien mencari faskes perujuk dengan mengetik minimal 3 huruf, misalnya "puskes" → "Puskesmas Kecamatan Kebayoran Baru (MMC-FRJ-009)". Tanggal surat tidak boleh melewati hari ini. Nomor surat, dokter perujuk, diagnosa, dan alasan diisi bila tertulis di surat. Pasien wajib mengunggah 1–3 foto/pindaian surat (PDF, JPG, PNG, masing-masing ≤ 5 MB). Terakhir, pasien memilih poli tujuan dan jadwal dokter pada tanggal kunjungan dengan kartu "Sisa kuota".
2. **General Consent** (khusus pasien baru) memakai langkah RJ apa adanya. Penanda tangan "Pasien Sendiri" terisi otomatis dari nama dan NIK di Data Pasien. Ada 12 butir klausul dan tanda tangan di layar sentuh.
3. **Verifikasi & Konfirmasi** menampilkan ringkasan: pasien lama hanya nama tersamar, sedangkan pasien baru dengan NIK tersamar (`************0002`). Isi ringkasan lainnya: kunjungan, rujukan, penjamin, consent, dan tujuan kode. Tombol **Kirim Pendaftaran** baru aktif sesudah "Data pendaftaran sudah benar" dicentang dan verifikasi keamanan selesai. Selama proses, tombol bertuliskan "Mengirim..." dan terkunci.
4. Bila backend menolak dengan `409`:
   - "Kuota dokter … sudah penuh" atau "Jadwal dokter tidak tersedia" → pasien dikembalikan ke langkah tempat jadwal dipilih (Data Kunjungan, atau Data Rujukan untuk Pasien Rujukan). Pesannya tampil di sana, dan kuota dimuat ulang.
   - "Anda sudah memiliki pendaftaran online pada tanggal ini" atau "Anda sudah terdaftar pada tanggal ini" → pasien dikembalikan ke Data Kunjungan.
   - Dalam kedua kasus, isian lain (data pasien, rujukan, berkas, penjamin, tanda tangan) tetap utuh.
5. **Selesai** menampilkan kode check-in besar (contoh `K7QM-4TRZ`), nomor pendaftaran, tanggal, poli, dokter, dan jam. Peringatan menjelaskan bahwa kode hanya tampil sekali. Bila kontaknya email, tampil "Kode sudah dikirim ke email Anda." atau "Email belum terkirim…". Bila kontaknya No. WhatsApp, tombol **Kirim kode ke WhatsApp saya** membuka `https://wa.me/62…?text=` dengan kode, tanggal, poli, dan dokter. Tombol **Salin Kode** menyalin kode, dan **Daftar Lagi** membuang seluruh isian beserta kodenya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`outpatient-general-consent-step.jsx`, `outpatient-general-consent.utils.js`, `outpatient-referral-step.jsx`, `outpatient-referral.utils.js`, `outpatient-verification-step.jsx`, `outpatient-registration.constants.js` (consent, rujukan), `priority-reservation-issued-panel.jsx`, `priority-reservation.utils.js` (`buildWhatsAppUrl`), `base-button.jsx`, `confirm-modal.jsx`, `filter-select.jsx` (pencarian server), `InstanceAxios.jsx` (FormData), `outpatient-registration.service.js` (pola unggah). Di backend: `OnlineRegistrationPublicService.Submit.cs` (rujukan, consent, berkas, pesan), `OnlineRegistrationPublicController.cs`, `OnlineRegistrationPublicDtos.cs`, `GeneralConsentSignerType.cs`, `OnlineRegistrationEmailStatus.cs`, `OnlineRegistrationOptions.cs`, `appsettings.json` (`OnlineRegistration`, `Email:Smtp`).

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/components/view/online-registration/online-registration-referral-step.jsx` | Baru | Langkah Data Rujukan publik, dirangkai dari `ReferralSectionHeader`, `ReferralDocumentsField`, `ScheduleSelect`, dan isian standar |
| `src/components/view/online-registration/online-registration-verification-step.jsx` | Baru | Ringkasan tersamar dengan gaya Verifikasi RJ, konfirmasi, `TurnstileWidget`, tombol Kirim/"Mengirim..." |
| `src/components/view/online-registration/online-registration-completed-step.jsx` | Baru | Kode, ringkasan, status email, tombol WhatsApp (`buildWhatsAppUrl`), Salin Kode, Daftar Lagi |
| `src/components/view/online-registration/online-registration-page.jsx` | Diubah | Merender Data Rujukan, General Consent RJ, Verifikasi, Selesai; penanda langkah sementara FE-002 dihapus |
| `src/lib/hooks/online-registration/use-online-registration.js` | Diubah | Berkas rujukan, validasi rujukan dan consent, isian awal penanda tangan, Kirim multipart dengan penjaga kirim ganda, arah kembali `409`, hasil di memori, Daftar Lagi; `hasReferralData` kini nyata |
| `src/lib/hooks/online-registration/use-online-registration-referral-institutions.js` | Baru | Pencarian faskes publik: minimal 3 huruf, ditunda 350 ms, request lama dibatalkan |
| `src/lib/services/online-registration/online-registration-public.service.js` | Diubah | `submitOnlineRegistration(formData)` dengan batas waktu 60 detik |
| `src/utils/online-registration/online-registration.utils.js` | Diubah | Opsi faskes, validasi berkas (`029`/`030`) dan rujukan (`027`/`028`), payload Kirim (enum angka, penanda tangan → angka), FormData, NIK tersamar, teks WhatsApp |
| `src/lib/constants/online-registration/online-registration.constants.js` | Diubah | Batas rujukan, peta penanda tangan, status email, nilai awal rujukan dan consent, pesan `KSK-VAL-PO-021` s.d. `034`; `STEP_PENDING` dihapus |
| `src/style/online-registration/online-registration.module.css` | Diubah | `.codePanel`, `.codeLabel`, `.codeValue`, `.completedActions` (token saja) |
| `outpatient-registration/outpatient-referral-step.jsx` | Diubah (ekspor + prop opsional) | `ReferralDocumentsField` dan `ReferralSectionHeader` diekspor; `ReferralDocumentsField` menerima `maxFiles` (bawaan 10), `label` (bawaan "Berkas Surat Rujukan (opsional)"), `required` (bawaan `false`) |

Tidak ada perubahan backend, migration, atau database.

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Penerapan |
| --- | --- |
| View tidak memanggil Axios | Kirim dan pencarian faskes lewat hook → service publik (`_skipAuthRefresh`) |
| Hook membersihkan request | `AbortController` untuk Kirim, pencarian faskes, pencocokan, dan penyedia; timer pencarian dibersihkan saat unmount |
| Satu fokus per hook | Pencarian faskes dipisah ke `use-online-registration-referral-institutions.js` |
| Utility murni | Payload, FormData, validasi, dan teks WhatsApp di `utils/online-registration` |
| Privasi (PO-FE.7) | Kode hanya di state `result`; tanpa URL, Redux, maupun storage; Daftar Lagi membuang semuanya; tanpa `console` |

### 3.4 Gerbang keputusan base component

UI GATE: 7 elemen — REUSE 2, EXTEND 2, COMPOSE 3, WRAP 0, NEW 0

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| General Consent | `OutpatientGeneralConsentStep` | murni, membaca isian form | REUSE | Apa adanya; isian awal lewat nama isian RJ |
| Kartu jadwal di Data Rujukan | `ScheduleSelect` | sudah EXTEND di FE-002 (`fullScheduleIds`) | REUSE | Sumber publik |
| Unggah surat rujukan | `ReferralDocumentsField` | `outpatient-referral-step.jsx` | EXTEND | Diekspor; `maxFiles`, `label`, `required` opsional |
| Judul bagian rujukan | `ReferralSectionHeader` | `outpatient-referral-step.jsx` | EXTEND | Diekspor saja |
| Langkah Data Rujukan | `OutpatientReferralStep` | bagian 1 | COMPOSE | View baru dari bagian-bagian di atas |
| Verifikasi + captcha + Kirim | `OutpatientVerificationStep` | bagian 1 | COMPOSE | Gaya `verificationGrid`/`detailList` RJ, `BaseCheckboxCard`, `TurnstileWidget` |
| Selesai | `BaseButton as="a"`, `InformationAlert`, `EmergencyInlineAlert` | pola `priority-reservation-issued-panel.jsx` | COMPOSE | View baru, sesuai PO-FE.4 |

Pilihan yang disajikan dan dijalankan:

- **Data Rujukan — A (rekomendasi, dijalankan): rangkai dari bagian langkah RJ yang diekspor.** Rupa sama dengan RJ, dan RJ petugas tidak berubah perilakunya. **B: EXTEND `OutpatientReferralStep` dengan ±10 prop varian** sesuai bunyi kolom Reuse roadmap. Opsi B ditolak karena menambah banyak cabang ke langkah petugas sepanjang 780 baris (Internal, Lab), dengan risiko regresi Rujukan RJ. Selain itu, hook data petugasnya berjalan tanpa syarat.
- **Verifikasi — A (rekomendasi, dijalankan): rangkai dengan gaya kartu verifikasi RJ.** **B: EXTEND `OutpatientVerificationStep` dengan ±8 prop** (baris pasien, slot captcha, label tombol, sembunyikan blok HR dan "Proses Penyimpanan").

Tidak ada elemen `NEW`, dan tidak ada `EXTEND` yang mengubah perilaku bawaan. Delta terhadap kolom Reuse roadmap ("`outpatient-referral-step` (varian sumber data)", "`outpatient-verification-step`") dicatat di sini: yang dipakai ulang adalah bagian-bagian langkah itu, bukan langkah utuhnya.

## 4. State yang ditangani di layar

| Keadaan | Penanganan |
| --- | --- |
| Memuat | Pencarian faskes "memuat"; poli "Memuat poli..."; jadwal "Memuat jadwal dokter..."; tombol Kirim "Mengirim..." |
| Kosong | Faskes: < 3 huruf → "Ketik minimal 3 huruf nama atau kode faskes."; tanpa hasil → "Faskes tidak ditemukan."; tidak ada poli → pesan + arahan mengubah tanggal |
| Gagal | Poli/jadwal: pesan + [Coba lagi]; Kirim `400`/`404`/`429`/`503`: pesan backend di Verifikasi, isian tetap, captcha diminta ulang |
| `409` | Kembali ke langkah jadwal (kuota/jadwal) atau Data Kunjungan (tanggal) dengan pesan; kuota dimuat ulang |
| Captcha belum siap | Komponen FE-001; tombol Kirim tetap nonaktif |
| Kirim ganda | Tombol terkunci + penjaga `submitInFlightRef`; Kembali juga terkunci selama proses |
| Email gagal | "Email belum terkirim. Simpan kode ini, atau gunakan Lupa Kode di kiosk rumah sakit saat check-in." |

## 5. Endpoint yang dikonsumsi

| Endpoint | Pemakaian |
| --- | --- |
| `POST /api/v1/health-services/registration-management/online-registrations/public` | `multipart/form-data`: `payload` (JSON `OnlineRegistrationSubmitRequest`, enum angka) + `referralDocuments` (khusus Rujukan) → `{ registrationNumber, code, visitDate, clinicName, doctorName, startTime, endTime, emailStatus, whatsAppNumber }` |
| `GET …/public/options/referral-institutions?search=` | Pencarian faskes perujuk |
| `GET …/public/options/clinics`, `…/doctor-schedules` | Poli dan jadwal di Data Rujukan (sama dengan Data Kunjungan) |

## 6. Verifikasi

Lingkungan uji: dev server frontend di port 3000, backend uji di `https://localhost:7185` (build Release dari scratchpad, kunci uji Turnstile "selalu lolos"), dan SMTP sink lokal port 2525 lewat `Email__Smtp__*`. Request browser ke 7184 dialihkan ke 7185. Ketiganya saya nyalakan sendiri, lalu saya matikan sesudah uji. Backend user di 7184 tidak disentuh. Data uji di `QuilvianNewDevSukma` dibuat lewat API publik. NIK dan kode tidak dicetak di log uji.

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` pada folder online-registration (app, view, hook, util, konstanta, service) dan `outpatient-referral-step.jsx` | PASS — 0 error, 0 warning |
| `npm run lint:errors` | PASS — 0 error |
| `npm run build` | PASS — 854/854 halaman, `ƒ /online-registration` |
| Playwright kiriman, HP 375 × 800 (`submit-flow.cjs`), run penuh | 28/29 PASS. S1k gagal karena skrip uji membaca email mentah, padahal isinya base64 (lihat catatan) |
| Playwright S1 ulang dengan pemeriksaan email yang mendekode base64 (`ONLY=S1`) | **13/13 PASS** |
| Playwright Pasien Baru + Asuransi kartu baru (`s4-new-insurance.cjs`) | **2/2 PASS** |
| Verifikasi data backend lewat API petugas `BE-KSK-PO-009` (`fe003_verify.py`, `fe003_verify_s4.py`) | **7/7 PASS** |
| Regresi menu RJ petugas (`rj-regression.cjs`, ditambah cek unggah Data Rujukan RJ) | **14/14 PASS** |
| Ulang uji halaman publik FE-002 (`public-flow.cjs`) | **38/38 PASS**, sesudah selektor "Kembali" diberi `.first()` karena Verifikasi kini punya dua tombol Kembali, sama seperti RJ |
| Grep konsistensi UI | Bersih: tanpa hex/rgba, tanpa ukuran huruf literal, tanpa `!important`, tanpa `fw-*/fs-*`, tanpa `<table>` mentah, tanpa inline style. `<button>` mentah hanya tombol langkah berkelas langkah RJ |
| Grep privasi (`console`, `localStorage`, `sessionStorage`) | 0 hasil |

Rincian uji kiriman:

| Butir | Bukti |
| --- | --- |
| S1 Pasien Baru + Pribadi + email | Penanda tangan terisi dari Data Pasien; tanpa tanda tangan → "Tanda tangan yang bertanda tangan wajib diisi."; ringkasan NIK tersamar; Kirim nonaktif sampai konfirmasi + captcha; **409 kuota → Data Kunjungan, tanggal dan poli tetap**; tanda tangan tetap sesudah kembali; **"Mengirim..." terkunci, klik ganda = 1 POST**; kode `XXXX-XXXX` + nomor + poli + tanggal; "Kode sudah dikirim ke email Anda." tanpa tombol WhatsApp; Salin menyalin kode; **email di SMTP uji berisi kode dan poli, tanpa NIK**; URL/storage tanpa kode, NIK, dan email; HP tanpa gulir mendatar |
| S2 Pasien Lama + Asuransi kartu milik pasien + WA | Ringkasan hanya nama tersamar, tanpa NIK/No. RM dan tanpa consent; **tombol WhatsApp `https://wa.me/6281234567890?text=` berisi kode, "25 November 2026", poli; tanpa NIK/No. RM/penjamin**; `rel=noopener`; tanpa status email; Daftar Lagi menghapus kode dari layar; **409 nyata pendaftaran ganda (`KSK-VAL-PO-023`) → Data Kunjungan dengan pesan, poli tetap** |
| S3 Pasien Baru + Rujukan + Pribadi/Tunai | Kosong → "Pilih faskes perujuk."; **berkas 6 MB → "Ukuran berkas maksimal 5 MB."; `.exe` → "Unggah PDF, JPG, atau PNG, paling banyak 3 berkas."; 2 berkas sah masuk; tambah sampai 4 ditolak**; pencarian faskes < 3 huruf diberi petunjuk, tanpa hasil diberi pesan kosong; tanggal surat besok nonaktif; ringkasan Rujukan lengkap; **409 kuota → Data Rujukan, faskes/nomor/berkas tetap**; kiriman multipart tersimpan |
| S4 Pasien Baru + Asuransi kartu baru | Kartu baru (AdMedika, nomor polis) di ringkasan; kiriman tersimpan |
| P | Tanpa galat halaman, tanpa pelanggaran CSP, tidak pernah ke `/login` |

Rincian verifikasi data backend (API petugas, SuperAdmin):

| Butir | Hasil |
| --- | --- |
| V1 `PO-20261120-0012`, `0016` (S1) | Menunggu Check-in, Pasien Baru, Pribadi/Tunai, `hasConsent = true`, `consentSignedAt` terisi (AC 14) |
| V2 `0016` | `emailStatus = 1` Terkirim |
| V3 `0013` (S2) | Pasien Lama, Asuransi kartu milik pasien (tersamar), kontak `08…`, `emailStatus = 0`, tanpa consent |
| V4/V5 `0014` (S3) | Rujukan: faskes, nomor `RJK/…`, dokter, diagnosa, alasan, tanggal surat `2026-11-19`, 2 berkas `foto-surat.png` + `surat-rujukan.pdf`, Tunai, consent (AC 14, AC 15) |
| V6 `0017` (S4) | Kartu asuransi baru tersimpan: penyedia AdMedika, nomor polis, pemegang, hubungan; consent |

```
MANUAL TEST: Playwright — kiriman 28/29 (run penuh) + S1 13/13 (ulang) + S4 2/2; data backend 7/7; regresi RJ 14/14; halaman publik FE-002 38/38
AUTOMATED TEST: SKIPPED (opsional) — tes otomatis baru tidak diwajibkan test-policy; bukti perilaku dari uji browser di atas
```

Catatan jalannya uji:

- Run pertama memakai `Smtp__*` sehingga backend uji mengirim ke server SMTP bawaan dan status email `Failed` (`PO-20261120-0010`). Kunci konfigurasi yang benar `Email:Smtp:*`, lalu backend uji dinyalakan ulang.
- Run pertama juga menemukan dua salah harapan di skrip: tanggal tampil "26 November 2026", dan kata "rumah" tidak punya hasil faskes di DB dev.
- S1k di run penuh gagal karena badan email base64. Skrip diperbaiki (dekode) lalu S1 diulang: 13/13.
- Satu run ulang sempat berjalan dengan skrip lama, karena patch skrip gagal di tahap penulisan. Run itu membuat `PO-20261120-0015` (S1), dan S2-nya ditolak `409` karena P1 sudah terdaftar pada 25 Nov (`0013`). Penolakan itu sesuai aturan satu pendaftaran per tanggal.

Pendaftaran uji yang terbentuk (semua Menunggu Check-in, kunjungan 25–26 Nov 2026): `PO-20261120-0010` s.d. `0017`. P1 (pasien lama BE-005) memegang `0011` (26 Nov) dan `0013` (25 Nov). Data ini ikut dibersihkan bersama fixture uji modul (lewat Batalkan `BE-KSK-PO-010` atau penutupan Tidak Datang).

Skrip dan tangkapan layar ada di scratchpad sesi (`fe-po003/submit-flow.cjs`, `s4-new-insurance.cjs`, `fe003_ledger_*.json`, `s1-verify-375.png`, `s1-done-375.png`, `s3-referral-375.png`; `po002/fe003_verify.py`, `fe003_verify_s4.py`) dan tidak di-commit.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Bukti | Status |
| --- | --- | --- |
| AC 5 (tampilan dan kiriman): kode dikirim lewat email; tombol WhatsApp `wa.me` berisi kode | S1i, S1k, V2; S2b | ✅ |
| AC 6 (tampilan dan kiriman): kontak kosong, captcha, ganda, > 30 hari, batas laju | Kontak/tanggal di layar (FE-002); captcha wajib sebelum Kirim (S1d); ganda → `409` dengan pesan (S2f); `400`/`429`/`503` Kirim ditampilkan dengan pesan backend (penanganan galat bagian 4; batas laju sendiri terbukti di `BE-KSK-PO-005`) | ✅ |
| AC 14 (tampilan dan kiriman): consent pasien baru tersimpan pada pendaftaran | S1a–S1f, V1, V4, V6 | ✅ |
| AC 15 (tampilan dan kiriman): berkas > 5 MB dan bukan PDF/JPG/PNG ditolak | S3b, V5 | ✅ |
| Tambahan 1: tombol Kirim terkunci selama proses | S1g | ✅ |
| Tambahan 2: `409` kuota kembali ke Data Kunjungan tanpa menghapus isian | S1e (Data Kunjungan), S3g (Rujukan → Data Rujukan, tempat jadwal dipilih), S2f (`409` nyata) | ✅ |
| Tambahan 3: kode tidak disimpan di storage | S1l | ✅ |

## 8. Catatan penutup

- **Pasien Rujukan dengan `409` kuota kembali ke Data Rujukan, bukan Data Kunjungan,** karena bagi Pasien Rujukan poli dan jadwal dipilih di Data Rujukan (urutan RJ). `409` karena tanggal tetap kembali ke Data Kunjungan.
- Klausul General Consent tercentang semua sejak awal, mengikuti bawaan RJ (`DEFAULT_OUTPATIENT_CONSENT_FORM_VALUES`, `KSK-DEC-034`: isi persetujuan mengikuti RJ). Pasien tetap dapat membatalkan centangnya. Bila pemilik ingin halaman publik dimulai tanpa centang, nilai awal online dapat dibedakan tanpa menyentuh RJ.
- `OutpatientGeneralConsentStep` RJ memakai warna pena `#09283e` literal di source RJ yang dipakai ulang. Nilai itu tidak disentuh task ini.
- Uji `400` captcha "selalu gagal" dan `429` Kirim tidak diulang di browser. Keduanya dibuktikan di `BE-KSK-PO-004`/`005`, dan di frontend jalurnya memakai penanganan galat umum yang sama dengan `404`/`409`.
- Halaman `/online-registration` kini lengkap dari awal sampai Selesai (`MVP-1`). Catatan go-live dari `FE-KSK-PO-001` tetap berlaku: `NEXT_PUBLIC_TURNSTILE_SITE_KEY` belum ada di `build.args` Docker (`KSK-PO-OQ-016`), dan konfigurasi SMTP produksi `Email:Smtp` adalah wewenang IT.
- Tidak ada commit, push, migration, atau eksekusi database langsung.

**Task berikutnya:** `FE-KSK-PO-007` (Daftar Pendaftaran Online petugas, dependency `BE-KSK-PO-009` ✅) dan `FE-KSK-PO-008` (Detail, Ubah Jadwal/Kontak, Batalkan). `FE-KSK-PO-005`/`006` menunggu `BE-KSK-PO-007` pada 2026-11-21.
