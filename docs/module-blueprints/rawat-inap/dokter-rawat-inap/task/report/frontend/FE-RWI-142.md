# Laporan Perubahan Frontend — `FE-RWI-142`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-142` |
| Judul | Salin A & P dari SOAP sebelumnya |
| Slice | Rencana kerja SOAP Dokter Rawat Inap Rev 2.1, gelombang 2 |
| Roadmap | [`rencana-kerja/soap/soap.md`](../../../roadmap/rencana-kerja/soap/soap.md) bagian 7.5; didaftarkan di [`frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) |
| Trace | Keputusan pemilik K4 (30-09-2026: "boleh"); `soap.md` bagian 3.4 butir 17, skenario 2 |
| Contract version | `0.6.1` + delta `BE-RWI-142` (diagnosa per catatan di lini masa) |
| Wewenang UI | Rencana kerja `soap.md` Rev 2.1 yang disetujui pemilik 30-09-2026. Batasnya: tombol salin, penanda, dan konfirmasi pada Form SOAP |
| Dependency | `FE-RWI-139` ✅ source 30-09-2026 (penyimpanan diagnosa) |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 1, berkas diubah 1 (6 berkas), logika 1, kontrak 1, database 0, keamanan 0, UI 0 |
| Task mode | `FRONTEND` (laporan di repository backend) |
| Target tulis | `QuilvianSystemFrontendDev/src/**` bagian SOAP rawat inap |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `82487a491` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `b342ae46` + perubahan `BE-RWI-142` di working tree |
| Tanggal | 30 September 2026 |
| Status | ✅ Selesai di tingkat source. ESLint dan test PASS. `npm run build` dan runtime **NOT RUN** |

---

## 1. Keadaan yang ditemukan di awal

V1 maupun V2 tidak punya cara menyalin catatan sebelumnya. Pada visite harian, diagnosa dan rencana terapi sering sama dengan kemarin, sehingga dokter mengetik ulang Assessment, Plan, dan memilih ulang diagnosa satu per satu. Pemilik menyetujui penyalinan **hanya A dan P beserta diagnosanya** (K4). S dan O adalah temuan hari ini dan tidak boleh disalin.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** dokter rawat inap pada catatan draf miliknya (skenario 2 `soap.md`: Tn. Budi Santoso, hari rawat ke-4).

1. Pada seksi **Pilih ICD-10** muncul tombol **Salin A & P dari SOAP 29/09 08.05**. Tombol hanya ada bila episode punya catatan **Final**; sumbernya catatan Final terakhir di lini masa.
2. Dokter menekan tombol itu.
   - Bila Assessment dan Plan masih kosong, penyalinan langsung terjadi.
   - Bila salah satunya sudah berisi, muncul konfirmasi di halaman: **"Timpa Assessment dan Plan?"**, dengan penjelasan bahwa diagnosa catatan sumber ditambahkan ke daftar. Pilihannya **Salin dan Timpa** atau **Batal**. Tidak ada isian yang ditimpa diam-diam.
3. Yang disalin:
   - **diagnosa aktif** catatan sumber (misalnya J18.9 Utama dan I10), dengan **Utama yang sama**. Diagnosa yang sudah ada di daftar tidak digandakan;
   - **narasi Assessment** dan **narasi Plan** catatan sumber. Kolom Plan terstruktur **dibentuk ulang** saat disimpan dari baris rekomendasi di Plan salinan yang rekomendasinya masih aktif (`ActiveForSoap`). Kolom itu tidak disalin mentah dari catatan sumber.
4. **S dan O tidak disalin** dan tetap berisi temuan hari ini.
5. Di bawah Assessment dan Plan muncul penanda **"Disalin dari SOAP 29/09 08.05"**. Penanda hilang begitu dokter menyunting teksnya.
6. Penyimpanan diagnosa hasil salinan:
   - catatan baru: disimpan bersama catatan saat Simpan Draf atau Selesaikan pertama;
   - catatan yang sudah tersimpan: langsung disimpan lewat API, lalu Utama disamakan dengan sumber.

**Jalur tidak normal.** Penyimpanan diagnosa salinan gagal: pesan server tampil, daftar dibaca ulang dari server, dan catatan tidak diselesaikan (aturan `FE-RWI-139`). Episode tanpa catatan Final: tombol tidak muncul. Draf milik dokter lain: tombol tidak muncul karena editor nonaktif.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`rules/frontend/*`; `use-inpatient-progress-note.jsx`; `use-progress-note-diagnoses.jsx`; base `ConfirmModal` (prop `show`, `variant`, `confirmLabel`, `cancelLabel`, `onConfirm`, `onCancel`, `onHide`); lini masa SOAP `BE-RWI-142`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-progress-note.jsx` | `copySource` (catatan Final terakhir), `requestCopyFromPrevious` (konfirmasi bila A/P berisi), `applyCopyFromPrevious`, `cancelCopyFromPrevious`, `copiedFrom` (penanda + status disunting) |
| `src/lib/hooks/health-services/inpatient-management/use-progress-note-diagnoses.jsx` | `copyDiagnoses`: lewati duplikat, Utama mengikuti sumber, simpan lokal (catatan baru) atau lewat API (catatan tersimpan) |
| `src/components/view/.../tabs/progress-note/soap-icd-section.jsx` | Tombol **Salin A & P dari SOAP {tanggal}** + keterangan "S dan O tidak disalin" |
| `src/components/view/.../tabs/progress-note/soap-editor.jsx` | Penanda "Disalin dari SOAP {tanggal}" di bawah A dan P |
| `src/components/view/.../tabs/progress-note/physician-progress-tab.jsx` | `ConfirmModal` konfirmasi timpa |
| `src/utils/health-services/inpatient-management/inpatient-progress-note-utils.jsx` | `findLatestFinalNote`, `formatShortDateTime` |

### 3.3 Kepatuhan arsitektur frontend

Logika salin ada di hook; view hanya memanggil `requestCopyFromPrevious`. Konfirmasi memakai `ConfirmModal`, bukan `window.confirm`.

**Tabel keputusan base component**

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Tombol salin | `REUSE` `BaseButton variant="ghost"` | Katalog base |
| Konfirmasi timpa | `REUSE` `ConfirmModal variant="warning"` | Katalog base |
| Penanda "Disalin dari SOAP" | `COMPOSE` `<span>` + CSS token | Tanpa komponen baru |

`UI GATE: PASS — 0 NEW, 0 EXTEND yang mengubah perilaku default; seluruh elemen REUSE/COMPOSE.`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol salin nonaktif selama diagnosa sedang disimpan (`busy`) |
| Kosong | Tidak ada catatan Final: tombol tidak tampil |
| Gagal | Pesan server penyimpanan diagnosa tampil di seksi ICD; daftar dibaca ulang dari server |
| Tanpa hak akses | Tombol tidak tampil bila editor tidak dapat disunting (bukan penulis, episode tertutup, atau tanpa hak tulis) |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/doctor-consultations/episodes/{episodeId}/soap-timeline` | Sumber salin: A, P, dan `diagnoses[]` catatan Final terakhir | `DoctorConsultation : Read` |

#### Health Services / Clinical Management / Patient Diagnosis

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/clinical-management/patient-diagnoses` | Simpan diagnosa hasil salinan | `PatientDiagnosis : Create` |
| `PATCH` | `/v1/health-services/clinical-management/patient-diagnoses/{id}/set-primary` | Samakan Utama dengan sumber | `PatientDiagnosis : SetPrimary` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <15 berkas SOAP> --quiet` lalu tanpa `--quiet` | Exit 0; 0 error, 0 peringatan | `PASS` | Keluaran ESLint 30-09-2026 |
| `inpatient-soap-modernization.test.mjs` | 12/12 lulus, termasuk "sumber salin A & P …": draf tidak menjadi sumber; catatan yang sedang dibuka dikecualikan | `PASS` | Keluaran `node --test` |
| Suite unit penuh | 2106 / 2101 lulus / 5 gagal (garis dasar, tidak terkait SOAP) | `EXISTING / ENVIRONMENT ISSUE` untuk 5 kegagalan | Lihat `FE-RWI-139` bagian 6 |
| Kompilasi Turbopack dev route dokter rawat inap | HTTP 200; tanpa stub galat | `PASS` | Lihat `FE-RWI-141` bagian 6 |
| Cek salah-prop `ConfirmModal show=` | Sesuai source komponen | `PASS` | `confirm-modal.jsx` |
| `npm run build` | — | `NOT RUN` | `next dev` pemilik berjalan; build dijalankan pemilik |
| Skenario 2 `soap.md` di peramban | — | `NOT RUN` | Butuh backend berjalan dan episode dengan catatan Final |

MANUAL TEST: NOT FEASIBLE — tidak ada sesi peramban berakun dokter pada sesi ini.

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-soap-modernization.test.mjs` — PASS (12/12).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (`soap.md` 7.5) | Status | Bukti |
| --- | --- | --- |
| 1. Catatan baru pada episode yang punya SOAP Final menampilkan tombol salin; episode tanpa catatan Final tidak | Terpenuhi di tingkat source | `copySource = findLatestFinalNote(items, …)`; tombol dirender hanya bila `copySource` ada; test fungsi |
| 2. Menyalin memasukkan diagnosa sumber dengan Utama yang sama, dan diagnosa itu tersimpan lewat API | Terpenuhi di tingkat source; runtime `NOT RUN` | `copyDiagnoses` (lokal → `persistPending` saat simpan; tersimpan → `POST` + `set-primary`) |
| 3. S dan O tetap kosong setelah menyalin | Terpenuhi | `applyCopyFromPrevious` hanya menulis `assessment` dan `plan` |
| 4. Isian A atau P yang sudah diketik tidak tertimpa tanpa konfirmasi | Terpenuhi | `requestCopyFromPrevious` membuka `ConfirmModal` bila A atau P berisi |
| DoD 2, 4, 5, 7 | Terpenuhi | Bagian 6; roadmap dan traceability diperbarui |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | (1) Tombol salin juga tampil pada draf yang sudah tersimpan, bukan hanya catatan baru. Sumbernya tetap catatan Final terakhir selain catatan itu sendiri. Perilaku ini tidak melanggar kriteria, dan tetap berguna bila dokter lupa menyalin sebelum menyimpan draf pertama. (2) `soap.md` 7.5 menyebut "kolom Plan terstruktur" ikut disalin. Yang terjadi, kolom itu dibentuk ulang dari baris rekomendasi yang masih aktif. Butir yang rekomendasinya sudah dinonaktifkan tetap ada di narasi Plan, tetapi tidak masuk kolom terstruktur. Pilihan ini disengaja: kolom terstruktur hanya memuat butir yang dicentang dari master aktif (`soap.md` 3.4) |
| Dependency backend | `BE-RWI-142` ✅ source (diagnosa per catatan di lini masa). Tanpa backend baru, hanya narasi A dan P yang tersalin |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi terpotong ringkasan konteks; dilanjutkan dari kondisi terverifikasi |
| Status Git | Lihat laporan `FE-RWI-141` bagian 8 |
| Langkah berikutnya | Pemilik menguji skenario 2 `soap.md` di peramban |
