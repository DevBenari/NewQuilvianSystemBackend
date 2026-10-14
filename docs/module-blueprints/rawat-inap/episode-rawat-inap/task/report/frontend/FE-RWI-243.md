# Laporan Perubahan Frontend — `FE-RWI-243`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-243` |
| Judul | Langkah Pilih Bed: unit admisi mengikuti bed terpilih dan alasan kegagalan tampil apa adanya |
| Slice | Perbaikan `PLAN-REPAIR-EPS-009` — `FIX-EPS-009-01` dan `FIX-EPS-009-02` |
| Roadmap | [`frontend-roadmap-admisi-pendaftaran.md`](../../../roadmap/frontend-roadmap-admisi-pendaftaran.md) bagian 3 dan 4.8 |
| Trace | `ISS-EPS-009-01`, `ISS-EPS-009-02`, `ISS-EPS-009-T4`; `RWI-DEC-268`; kontrak `RWI-KELAS-006-1.0`; `05-skema-tampilan.md` 3.8–3.9 dan amandemen `PLAN-REPAIR-DOK-006` |
| Contract version | API `0.11.0` `approved` + `RWI-KELAS-006-1.0` (`POST /episodes` `SelectedBedId`); tidak ada endpoint baru |
| Wewenang UI | Tidak ada perubahan tampilan. Hanya isi alert galat yang sudah ada dan nilai unit pada payload |
| Dependency | Tidak ada task backend; server existing sudah menerima unit dari kamar bed (`InpEpisodeService.OpenAdmissionAsync`) |
| Klasifikasi | `MEDIUM` — 3 berkas logika di satu alur, tanpa JSX/CSS, perilaku titik tulis 1 berubah |
| Task mode | `FRONTEND` (laporan di repository backend sesuai `AGENTS.md`) |
| Target tulis | `QuilvianSystemFrontendDev`: dua hook admisi, satu utility, satu test unit |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `c2f42e4ce3af5e4d07ec2c963bcec3e97a4031aa` (`HamzahV2`), working tree bersih sebelum task |
| Commit backend yang dijadikan rujukan | `86cb8ef8d3660e97776096fb33fcaf7dec7c9006` (`MHamzah`) |
| Tanggal | 13 Oktober 2026 |
| Status | ✅ Selesai pada tingkat source dan test unit terarah. ESLint dan `npm run build` `NOT RUN` karena `node_modules` tidak ada di repository saat task dikerjakan |

---

## 1. Keadaan yang ditemukan di awal

Sejak `FE-RWI-240` (13 Oktober 2026) kunjungan dan episode rawat inap baru dibuat ketika petugas
menekan **Lanjut ke Pemesanan** di langkah Pilih Bed. Tombol itu selalu gagal dengan kalimat
"Admisi belum dapat disimpan. Periksa data kamar dan isian Dokter sebelum melanjutkan."
Buktinya (lihat `ISSUE-EPS-009`):

1. Efek reset di `use-inpatient-admission-doctor.jsx` mengosongkan `serviceUnitId` setiap kali
   pasien dipilih, sedangkan unit bawaan hanya diisi sekali saat halaman dibuka.
2. `resolveInpatientAdmissionRoom(bed, form.serviceUnitId)` membandingkan unit bed dengan unit
   kosong itu, lalu menolak "Kamar yang dipilih berbeda dari unit layanan admisi".
3. Alasan itu ditulis ke `formError` langkah Dokter yang tidak tampil di langkah Pilih Bed;
   `prepareBooking` menggantinya dengan kalimat umum.
4. Temuan tambahan `ISS-EPS-009-T4`: kode galat server dibaca dari `data.code`, padahal
   `InpatientEpisodeController.FromStatus` mengirimnya di `data.errors.code`. Akibatnya setiap 409 —
   termasuk "Kunjungan ini sudah punya episode rawat inap." — tampil sebagai "Pasien memiliki
   permintaan admisi dari kamar pulih".

---

## 2. Proses bisnis dari sisi pengguna

Petugas admisi (jalur Pasien Baru, Pasien Lama, atau Kamar Pulih):

1. Memilih pasien, cara bayar, deposit bila Tunai, lalu DPJP di langkah Dokter.
2. Di langkah Pilih Bed, papan tersaring ke unit rawat inap bawaan. Petugas boleh mengganti
   penyaring unit, kamar, kelas, atau mencari kode bed.
3. Memilih satu bed yang dapat dipilih, lalu menekan **Lanjut ke Pemesanan**.
4. Layar membuat kunjungan dan episode dengan **unit dan kelas dari kamar bed itu**, lalu maju ke
   Booking Bed.

Jalur tidak normal:

- Bed tanpa kelas atau tanpa unit: layar menolak dengan kalimat yang menyebut kelas/unit.
- Server menolak (misalnya unit bukan rawat inap, pasien punya permintaan kamar pulih, kunjungan
  sudah dipakai): kalimat server tampil di langkah Pilih Bed; isian Dokter dan bed terpilih tetap.
- Kunjungan sudah terbentuk tetapi episode gagal: kalimat server tampil; menekan lagi memakai
  kunjungan yang sama.

Contoh: Tn. Budi (pseudonim), DPJP dr. Andi, bed `RI-201-A` di unit "Rawat Inap" Kelas II →
`POST /patient-encounters/admin` (unit Rawat Inap, kelas II) → `POST /episodes`
(`selectedBedId` = `RI-201-A`) → Booking Bed. Bila petugas mengganti penyaring ke unit ICU dan
memilih `ICU-01`, unit admisinya ICU.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`inpatient-admission-view.jsx`, `inpatient-admission-bed-step.jsx`,
`inpatient-admission-doctor-step.jsx`, `use-inpatient-admission-bed.jsx`,
`use-inpatient-admission-doctor.jsx`, `use-inpatient-bed-board.jsx`,
`use-inpatient-admission-transfer.js`, `inpatient-admission-transfer-utils.js`,
`inpatient-admission-encounter-utils.jsx`, `inpatient-bed-utils.jsx`,
`inpatient-admission-encounter.service.js`, `inpatient-admission-flow-constants.jsx`,
`tests/unit/inpatient-class-coverage.test.mjs`; backend `InpEpisodeService.cs` (`OpenAdmissionAsync`,
`ValidateServiceUnitAndPatientClassAsync`), `InpatientEpisodeController.cs` (`FromStatus`),
`Responses/ApiResponse.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx` | `resolveInpatientAdmissionRoom(bed)` mengembalikan `serviceUnitId`, `patientClassId`, `selectedBedId` dari kamar bed; perbandingan dengan unit tersembunyi dihapus; bed tanpa unit ditolak dengan kalimat yang jelas |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` | Unit bawaan disimpan di `defaultServiceUnitIdRef` dan dipulihkan saat pasien berganti; `submit` memakai unit dari bed, menyelaraskan isian, dan setiap kegagalan mengembalikan `message`; kode galat server dibaca dari `errors.code`, hanya `INP-ADM-REF-001` yang memakai kalimat kamar pulih; cabang pencarian unit cadangan yang tidak lagi terpakai dihapus |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` | `prepareBooking` menampilkan `result.message`; kalimat umum hanya bila kosong |
| `tests/unit/inpatient-class-coverage.test.mjs` | Test lama "unit berbeda ditolak" diganti aturan baru: unit mengikuti kamar, unit kosong ditolak; satu test baru untuk bed di unit rawat inap lain |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi tetap `view → hook → utils/service`. Perubahan logika ada di hook dan utility
murni; tidak ada view, JSX, CSS, Redux, atau service baru. Aturan "unit dan kelas dari bed" memakai
pola yang sudah ada pada `buildTransferAdmissionPayload` (jalur transfer). Komponen yang menampilkan
pesan tetap `InformationAlert` yang sudah dipakai langkah Pilih Bed.

**Gerbang keputusan base component**

| Elemen | Status | Bukti |
| --- | --- | --- |
| Alert galat langkah Pilih Bed | `REUSE` | `InformationAlert` existing, `inpatient-admission-bed-step.jsx:80`; hanya isi pesannya yang berubah |

`UI GATE: NOT APPLICABLE` — tidak ada elemen tampilan baru maupun perubahan perilaku base component.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol berlabel "Menyimpan admisi…" (tidak berubah) |
| Kosong | Tidak berubah (papan kosong sesuai penyaring) |
| Gagal | Alert merah berisi alasan asli, misalnya "Unit tujuan untuk kunjungan rawat inap wajib merupakan unit layanan bertipe rawat inap."; kalimat umum hanya bila alasan kosong |
| Tanpa hak akses | Tidak berubah; pesan 403 server tampil apa adanya |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Nilai `serviceUnitId` pada dua endpoint lama kini berasal dari kamar bed.

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/patient-encounters/admin` | Kunjungan rawat inap dengan unit dan kelas dari bed | `PatientEncounter : Create` (existing) |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/inpatient-management/episodes` | Episode Draft dengan `selectedBedId`; server memvalidasi unit/kelas kamar | `InpatientEpisode : Create` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-class-coverage.test.mjs` | 6/6 lulus, termasuk 2 test aturan unit dari bed | `PASS` | Keluaran perintah 13-10-2026 |
| Parse kedua hook lewat loader test | Kedua berkas terurai; berhenti di `Cannot find package 'react'` (sesudah parsing) | `PASS` (sintaks) | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | 2.604 test, 2.545 lulus, 59 gagal | `EXISTING / ENVIRONMENT ISSUE` | 29 kegagalan `Cannot find package 'axios'/'react'` karena `node_modules` tidak ada; sisanya test lama yang tidak menyentuh berkas task ini (`ward-pre-op`, `inpatient-admission-workspace`, `inpatient-newborn-admission`, `inpatient-bed-status`) |
| `npx eslint <4 berkas>` | Tidak dapat dijalankan: `eslint` tidak terpasang (`node_modules` kosong) | `NOT RUN` | `ERR_MODULE_NOT_FOUND` dari `eslint.config.mjs` |
| `npm run build` | Tidak dijalankan, alasan sama | `NOT RUN` | — |
| Uji layar Pilih Bed → Booking Bed | Tidak dijalankan | `NOT RUN` | Dev server tidak berjalan dan `node_modules` tidak ada |

Uji manual: `NOT FEASIBLE` — `node_modules` dan `.next` tidak ada di repository frontend sejak
operasi Git pemilik pukul 14.16–14.18; memasang ulang dependency di luar wewenang task.

**Tidak dijalankan:** ESLint, `npm run build`, dan uji layar. Sesuai keputusan pemilik
10 September 2026 (build dijalankan pemilik sendiri), butir ini dicatat `NOT RUN`, bukan diklaim
lulus. Perintah untuk pemilik ada di bagian 8.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (`PLAN-REPAIR-EPS-009` bagian 4) | Status | Bukti |
| --- | --- | --- |
| `FIX-01` AC 1 — Pasien Lama → Tunai → DPJP → bed Rawat Inap Kelas II → kunjungan dan episode memakai unit, kelas, bed itu, lalu maju ke Booking Bed | Terpenuhi pada source; runtime `NOT RUN` | `use-inpatient-admission-doctor.jsx` `submit`: `effectiveServiceUnitId = roomContext.serviceUnitId`, `patientClassId: resolvedClassId`, `selectedBedId: roomContext.selectedBedId`; `inpatient-admission-bed-step.jsx:110-112` memanggil `onNext` saat berhasil |
| `FIX-01` AC 2 — bed di unit rawat inap lain dipakai, bukan ditolak "berbeda unit" | Terpenuhi | `resolveInpatientAdmissionRoom` tanpa perbandingan unit; test "FE-RWI-243: bed di unit rawat inap lain…" `PASS` |
| `FIX-01` AC 3 — unit bawaan tetap ada setelah pasien dipilih | Terpenuhi pada source | `defaultServiceUnitIdRef` diisi saat opsi halaman 1 tanpa kata kunci dimuat; efek reset memakai nilainya |
| `FIX-01` AC 4 — bed tanpa kelas/unit ditolak dengan kalimat kelas/unit | Terpenuhi | Test "kelas dan unit berasal dari kamar…" `PASS` (`/kelas perawatan/`, `/unit layanan/`) |
| `FIX-02` AC 1 — penolakan server tampil apa adanya | Terpenuhi pada source | `submit` → `fail(error.message)` / `fail(getInpatientSettingErrorMessage(...))`; `prepareBooking` → `setActionError(result.message)` |
| `FIX-02` AC 2 — permintaan kamar pulih tampil di langkah Pilih Bed | Terpenuhi pada source | `serverCode` dari `errors.code`; `INP-ADM-REF-001` → kalimat kamar pulih dikembalikan sebagai `message` |
| `FIX-02` AC 3 — kalimat umum hanya bila tidak ada alasan | Terpenuhi | `use-inpatient-admission-bed.jsx` `prepareBooking` |

DoD yang belum terpenuhi: ESLint dan build `NOT RUN` (lingkungan), uji layar `NOT RUN`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Repository frontend sempat berpindah ke `QuilvianDevV2` dengan merge berjalan (14.16–14.18). Source baru ditulis setelah kembali ke `HamzahV2` bersih. `node_modules` dan `.next` hilang sejak saat itu |
| Masalah yang diketahui | Bila master bed memuat bed pada unit bukan rawat inap dan petugas memilihnya, penolakan datang dari server ("Unit tujuan untuk kunjungan rawat inap wajib…") — kini terbaca jelas |
| Dependency backend | Tidak ada. Penyaring papan (`FIX-EPS-009-04`) dan kinerja (`FIX-EPS-009-03`) dikerjakan task backend terpisah; layar ini sudah mengirim penyaringnya |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali sesi terputus sebelum implementasi; keadaan Git diperiksa ulang, tidak ada pekerjaan ganda |
| Status Git | ` M src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx`<br>` M src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx`<br>` M src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx`<br>` M tests/unit/inpatient-class-coverage.test.mjs` |
| Langkah berikutnya | Pemilik: `npm ci` (atau `npm install`), `npx eslint src --quiet`, `npm run build`, lalu uji bagian 8 `PLAN-REPAIR-EPS-009` butir 1–3 |
