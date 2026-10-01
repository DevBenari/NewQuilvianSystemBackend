# Laporan Perubahan Frontend — `FE-RWI-140`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-140` |
| Judul | Panel tanda vital dan Pengawasan Harian; isian manual dokter |
| Slice | Rencana kerja SOAP Dokter Rawat Inap Rev 2.1, gelombang 2 |
| Roadmap | [`rencana-kerja/soap/soap.md`](../../../roadmap/rencana-kerja/soap/soap.md) bagian 7.3; didaftarkan di [`frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) |
| Trace | Permintaan pemilik 2 dan 4; keputusan K5; cacat C9; `soap.md` bagian 3.2 |
| Contract version | `0.6.1` + delta `BE-RWI-141` (`soap.md` bagian 4.2), disetujui pemilik 30-09-2026 |
| Wewenang UI | Rencana kerja `soap.md` Rev 2.1 yang disetujui pemilik 30-09-2026. Batasnya: kartu Tanda Vital pada Form SOAP rawat inap |
| Dependency | Endpoint deret tanda vital dan Pengawasan Harian yang sudah ada; `BE-RWI-141` (`sourceVitalSignId`, `isDoctorMeasuredVitalSign`) — ✅ source 30-09-2026 |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1 (6 berkas), logika 2, kontrak 1, database 0, keamanan 0, UI 1 |
| Task mode | `FRONTEND` (laporan di repository backend) |
| Target tulis | `QuilvianSystemFrontendDev/src/**` bagian SOAP rawat inap |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `82487a491` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `b342ae46` + perubahan `BE-RWI-141` di working tree |
| Tanggal | 30 September 2026 |
| Status | ✅ Selesai di tingkat source. ESLint dan test PASS. `npm run build` dan runtime **NOT RUN** |

---

## 1. Keadaan yang ditemukan di awal

- Tanda vital perawat sudah tersimpan per episode (`TrxPatientVitalSign`, ringkasan Pengawasan Harian), tetapi Form SOAP rawat inap meminta dokter **mengetik ulang**. V1 dulu menariknya otomatis dari skrining; fitur itu hilang di V2 dan tidak tercatat di Rev 1.
- Badge tanda vital V2 menyala menurut ada tidaknya ketikan, bukan menurut ada tidaknya data perawat seperti di V1.
- Tidak ada SpO2, kesadaran/GCS, EWS, tren, nyeri, GDS, maupun balans cairan di Form SOAP.
- Normalizer angka bawaan `BaseTextField` membuang titik desimal, sehingga suhu 37,9 tersimpan 379.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** dokter rawat inap saat visite. Pembaca turunannya adalah perawat ruangan, yang melihat ukuran dokter di grafik tanda vital.

1. Dokter membuka Form SOAP. Layar memuat deret tanda vital 24 jam episode dan ringkasan Pengawasan Harian hari ini secara bersamaan.
2. **Ada data perawat (skenario 1 `soap.md`):**
   - badge hijau **Data Tanda Vital Terbaru** menyala;
   - sumber dan jam tertulis, misalnya *"Dicatat Ns. Rina · 06.10 (1 jam lalu)"*;
   - nilai tampil: TD, Nadi, Napas, Suhu, SpO2 (dengan tanda O2), Kesadaran/GCS, TB, BB, EWS beserta tingkat risikonya;
   - baris kritis/abnormal diberi badge merah.
3. Baris **24 jam** menampilkan suhu tertinggi, TD terendah, nadi tertinggi, dan SpO2 terendah beserta jamnya. Contoh: demam 38,6 °C pukul 02.00 tetap terbaca walau nilai pagi 36,8 °C. Baris **Pengawasan harian** menampilkan nyeri terakhir, GDS terakhir, dan balans cairan.
4. Pada catatan baru, **Objective langsung terisi blok TTV**:

   ```text
   TTV (06.10, Ns. Rina): TD 100/60 mmHg · N 124 x/m · RR 32 x/m · S 37,9 °C · SpO2 96% O2 · EWS 3
   24 jam: suhu maks 38,6 °C (02.00) · Nyeri 2/10 (05.00)
   ```

   Dokter menulis pemeriksaan fisik di bawahnya. Selama blok itu belum disunting dokter, pemuatan ulang data memperbarui bloknya. Bila sudah disunting, blok dibiarkan dan tombol **Masukkan ke Objective** menaruh blok baru di atas.
5. Bila perawat mencatat data yang lebih baru saat form terbuka, muncul *"Ada data tanda vital lebih baru pukul 08.00 (Ns. Dewi)."* beserta tombol **Pakai data terbaru**.
6. **Tidak ada data perawat (skenario 4):** badge oranye **Isi Tanda Vital**, dan isian manual langsung terbuka (TD sistolik/diastolik, nadi, napas, suhu, SpO2, TB, BB). Suhu dan berat/tinggi menerima koma desimal (37,9 tetap 37,9). Setelah terisi, badge berganti menjadi **Ukuran Dokter**.
7. **Simpan Draf** mengirim:
   - data perawat: `sourceVitalSignId` baris yang dipakai;
   - ukuran dokter: `isDoctorMeasuredVitalSign = true` beserta nilainya. Backend (`BE-RWI-141`) mencatatnya ke deret tanda vital pasien dengan label Dokter, satu baris per catatan.
8. Membuka ulang catatan menampilkan sumber yang sama. Bila baris perawat yang dirujuk sudah di luar jendela 24 jam, kartu memakai snapshot catatan itu sendiri beserta nama perawat dan jamnya.

**Jalur tidak normal.**
- Akun tanpa izin baca tanda vital: *"Akses tanda vital perawat ditolak — Akun ini tidak memiliki izin membaca deret tanda vital. Tanda vital tetap dapat diisi manual."*
- Deret gagal dimuat: *"Tanda vital perawat tidak dapat dimuat."* beserta tombol **Muat ulang**.
- Ringkasan Pengawasan Harian gagal: kartu tetap tampil tanpa baris itu. Kegagalan ringkasan tidak menggagalkan kartu.
- Tab peramban aktif lagi: data dimuat ulang otomatis. V2 belum punya hub realtime tanda vital.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`rules/frontend/*` (lihat `FE-RWI-139`); `daily-monitoring.service.js` (`getPatientVitalSignsByEpisodeId`, `getDailyMonitoringSummary`); base `BaseTextField` (normalizer), `InformationAlert`, `ClinicalScoreBadge`, `ClinicalStatusBadge`; backend `PatientVitalSignController`, `InpatientVitalSignService`, `DailyObservationController`; source V1 `useVitalSignLatest`, `usePainAssesmentLatest`, `form-soap.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-progress-note-vital-signs.jsx` (baru) | Muat deret dan ringkasan harian bersamaan (`allSettled`), bedakan akses ditolak dan gagal, muat ulang saat tab aktif, `latest`, `trend`, `findById`, `ready` |
| `src/components/view/.../tabs/progress-note/vital-signs-card.jsx` | Ditulis ulang: badge V1, sumber dan jam, nilai + EWS, tren 24 jam, Pengawasan Harian, isian manual, data lebih baru, tombol aksi |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-progress-note.jsx` | Mode sumber (perawat/manual), blok Objective terlindungi, pilih otomatis data perawat terbaru pada catatan baru, snapshot catatan sebagai cadangan di luar 24 jam, `sourceVitalSignId`/`isDoctorMeasuredVitalSign` di payload |
| `src/utils/health-services/inpatient-management/inpatient-progress-note-utils.jsx` | `normalizeDecimalInput`, `normalizeVitalSignSeries`, `pickLatestVitalSign`, `summarizeVitalTrend`, `normalizeDailyMonitoringSummary`, `buildObjectiveVitalBlock`, payload tanda vital |
| `src/lib/constants/health-services/inpatient-management/inpatient-progress-note-constants.jsx` | `VITAL_SIGNS_FIELDS` (+ SpO2, jenis integer/desimal), `VITAL_SIGN_MODE`, `VITAL_SIGN_SOURCE`, label dan nada EWS, label kesadaran |
| `src/components/view/.../tabs/progress-note/progress-note-detail.jsx` | Catatan final menampilkan **Sumber Tanda Vital** ("Ns. Rina · 30/09 06.10" atau "Ukuran dokter · …") |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `view → hook → service yang sudah ada`. Tidak ada service atau slice baru. Normalizer desimal dipasang lewat `field.normalizeValue` milik `BaseTextField`, tanpa mengubah perilaku bawaannya.

**Tabel keputusan base component**

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Kartu Tanda Vital | `REUSE` `ClinicalSectionPanel layout="document"` | Base klinis |
| Badge hijau/oranye, Kritis/Abnormal, risiko EWS | `REUSE` `ClinicalStatusBadge` (`success`, `waiting`, `consultation`, `danger`) | Base klinis |
| Skor EWS | `REUSE` `ClinicalScoreBadge` | Base klinis |
| Isian manual | `REUSE` `BaseTextField` dengan `normalizerType: "number"` atau `normalizeValue` | Katalog base; opsi resmi komponen |
| Akses ditolak / gagal | `REUSE` `InformationAlert` | Katalog base |
| Grid nilai TTV | `COMPOSE` elemen native + CSS token | Tanpa komponen baru |
| Tombol Masukkan ke Objective / Isi manual / Pakai data / Muat ulang | `REUSE` `BaseButton` | Katalog base |

`UI GATE: PASS — 0 NEW, 0 EXTEND yang mengubah perilaku default; seluruh elemen REUSE/COMPOSE.`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat tanda vital perawat..." di baris status; tombol Muat ulang dalam keadaan memuat |
| Kosong | Badge oranye **Isi Tanda Vital**, "Belum ada tanda vital perawat pada 24 jam terakhir.", isian manual terbuka |
| Gagal | "Tanda vital perawat tidak dapat dimuat." beserta Muat ulang; isian manual tetap dapat dipakai |
| Tanpa hak akses | "Akses tanda vital perawat ditolak" dengan penjelasan bahwa isian manual tetap dapat dipakai |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Patient Vital Sign

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/patient-vital-signs/episodes/{episodeId}` | Deret 24 jam (bawaan backend), termasuk ukuran dokter | `PatientVitalSign : Read` |

#### Health Services / Clinical Management / Daily Monitoring

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/daily-monitoring/episodes/{episodeId}/summary` | Nyeri terakhir, GDS, balans cairan hari ini | `DailyObservation : Read` |

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/clinical-management/doctor-consultations` | Catatan baru dengan `sourceVitalSignId` atau `isDoctorMeasuredVitalSign` | `DoctorConsultation : Create` |
| `PATCH` | `/v1/health-services/clinical-management/doctor-consultations/{id}/soap` | Simpan draf dengan sumber tanda vital yang sama | `DoctorConsultation : WriteSoap` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <15 berkas SOAP> --quiet` lalu tanpa `--quiet` | Exit 0; 0 error, 0 peringatan | `PASS` | Keluaran ESLint 30-09-2026 |
| `npx eslint src --quiet` | 1 error di berkas Tab Tindakan yang tidak disentuh | `EXISTING / ENVIRONMENT ISSUE` | Lihat `FE-RWI-139` bagian 6 |
| `inpatient-soap-modernization.test.mjs` | 12/12 lulus: desimal suhu, blok TTV (jam, sumber, tren, nyeri), deret (baris batal diabaikan, label Dokter), payload perawat/manual | `PASS` | Keluaran `node --test` |
| Suite unit penuh | 2106 / 2101 lulus / 5 gagal (sama dengan garis dasar, tidak terkait SOAP) | `EXISTING / ENVIRONMENT ISSUE` untuk 5 kegagalan | Lihat `FE-RWI-139` bagian 6 |
| Kompilasi Turbopack dev route dokter rawat inap | HTTP 200; chunk `progress-note` tanpa stub galat; hook tanda vital ter-bundel | `PASS` | Pengambilan chunk 30-09-2026 |
| Grep konsistensi UI dan cek salah-prop | Nihil / sesuai | `PASS` | Lihat `FE-RWI-139` bagian 6 |
| `npm run build` | — | `NOT RUN` | `next dev` pemilik berjalan; build dijalankan pemilik |
| Kriteria 1–6 di peramban dengan data episode nyata | — | `NOT RUN` | Butuh backend berjalan dan akun dokter |

MANUAL TEST: NOT FEASIBLE — tidak ada sesi peramban berakun dokter pada sesi ini.

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-soap-modernization.test.mjs` — PASS (12/12).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (`soap.md` 7.3) | Status | Bukti |
| --- | --- | --- |
| 1. TTV perawat pukul 06.10 menampilkan nilai itu, nama perawat, dan jamnya, dengan badge hijau | Terpenuhi di tingkat source; runtime `NOT RUN` | `VitalSignsCard`: `describeVitalSignObserver` + `formatClock`; badge `vitalLatestBadge` nada `success` |
| 2. Tanpa TTV: badge oranye "Isi Tanda Vital", input manual aktif | Terpenuhi di tingkat source | Hook memindah catatan baru ke mode manual bila deret kosong; badge `vitalEmptyBadge` nada `waiting` selama isian manual masih kosong |
| 3. Suhu 38,6 pukul 02.00 dan 36,8 pukul 06.10 → "suhu maks 38,6 (02.00)" | Terpenuhi | `summarizeVitalTrend` + `formatTrendLine`/`buildObjectiveVitalBlock`; test blok TTV (`24 jam: suhu maks 38,6 °C (02.00) …`) |
| 4. Objective yang disunting dokter tidak ditimpa saat data dimuat ulang | Terpenuhi | `objectiveBlockRef` + `replaceGeneratedPrefix` (mengembalikan `null` bila blok disunting); test penggantian blok |
| 5. SpO2 tersimpan dan tampil lagi saat catatan dibuka kembali | Terpenuhi di tingkat source; runtime `NOT RUN` | Payload `oxygenSaturation`; `normalizeProgressNoteTimelineItem` dan `buildProgressNoteForm` membacanya; snapshot catatan sebagai cadangan kartu |
| 6. Simpan Draf dengan TTV manual → satu baris bersumber Dokter; menyimpan lagi tidak menambah baris | Terpenuhi di tingkat source (FE mengirim `isDoctorMeasuredVitalSign`; backend `BE-RWI-141` memperbarui baris yang sama); runtime `NOT RUN` | `buildVitalPayload`; `DoctorConsultationVitalSignService.RecordDoctorMeasuredAsync` |
| DoD 2, 4, 5, 7 | Terpenuhi | Bagian 6; roadmap dan traceability diperbarui |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Layar perawat belum memberi label "Dokter" pada baris ukuran dokter dan masih menampilkan tombol Ubah/Batal. Backend menolak perubahannya (`DOCTOR_VITAL_SIGN_READ_ONLY`). Perlu task frontend keperawatan tersendiri |
| Dependency backend | `BE-RWI-141` ✅ source; migration sudah di `QuilvianNewDevHamzah`. Backend versi ini memetakan kolom `SourceVitalSignId` pada **setiap** kueri `TrxDoctorConsultation`. Di database yang belum menerima migration `20260930110000_AddDoctorConsultationSourceVitalSign`, seluruh fitur catatan dokter akan gagal, bukan hanya field baru |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi terpotong ringkasan konteks; dilanjutkan dari kondisi terverifikasi |
| Status Git | Lihat laporan `FE-RWI-141` bagian 8 |
| Langkah berikutnya | Pemilik menguji skenario 1 dan 4 `soap.md` di peramban, lalu memeriksa grafik tanda vital perawat untuk baris ukuran dokter |
