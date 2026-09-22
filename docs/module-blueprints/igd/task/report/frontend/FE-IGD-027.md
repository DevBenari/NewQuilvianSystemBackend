# `FE-IGD-027` — Layar triase memakai riwayat penugasan dokter

| Field | Nilai |
| --- | --- |
| Task | `FE-IGD-027` |
| Gelombang | `EPIC IGD-04` · slice `IGD-S06` |
| Status | ✅ **SELESAI — dinaikkan kembali 21 September 2026 atas keputusan pemilik, sesudah `npm run build` revisi terbaru lulus** (dijalankan pemilik: `Compiled successfully`, 362/362 halaman, `postbuild` standalone berhasil; commit frontend `16c767916`). Sebelumnya 🟡 pada hari yang sama karena source berubah sesudah verifikasi pemilik (bagian 8); riwayat penilaian itu dipertahankan di bawah. Tampilan baris legacy "Data historis" **tetap belum terlihat di layar** karena dev 0 baris legacy — diterima pemilik sebagai catatan, bukan penghalang. Bukti dicatat **per revisi**; bukti revisi lama tidak dipakai untuk revisi terbaru. **Implementation Complete = ya** (revisi terbaru). **Scoped eslint = PASS** dan **unit test IGD = 38/38 PASS** (bagian 8.3; tidak dijalankan ulang pada penilaian ini). **Runtime inti = PASS 18 September 2026** pada revisi `3213419a7` — tiga belas pemeriksaan lewat layar, dijalankan pemilik — [evidence](../evidence/2026-09-18-verifikasi-runtime-fe-igd-027.md); **tidak diulang** untuk revisi terbaru. **Build penuh revisi terbaru = belum diverifikasi pemilik.** Tampilan baris legacy "Data historis" belum diuji lewat layar (menunggu migration `BE-IGD-048`). **UAT belum dan tidak diklaim** |
| Frontend | branch `RizkiV2` `3213419a7` |
| Requirement | `FR-IGD-016` sampai `FR-IGD-021`, sisi tampilan |
| Kontrak | API `0.7.0` bagian 3, 3.1, 3.2 — dipakai persis seperti yang dibangun `BE-IGD-045` |
| Dependency | `BE-IGD-045` ✅ **Build Verified + Runtime Verified** 17 September 2026 (12/12 skenario `PASS`) |
| Backend disentuh | **Nol** |

---

## 1. Perubahan inti

Penetapan dokter IGD berpindah dari endpoint Registrasi ke kontrak `EmergencyDoctorAssignment`.

Bedanya bukan sekadar alamat. `PATCH /patient-encounters/{id}/doctor` hanya menimpa satu kolom
dan **tidak meninggalkan riwayat**; kontrak baru menyimpan riwayat tambah-saja, sehingga layar
kini dapat menampilkan siapa dokter sebelumnya dan mengapa dialihkan — yang memang dituntut
`IGD-DEC-082`.

### 1.1 Identitas yang dipakai berubah

Kontrak baru memakai **id kunjungan IGD**, bukan id encounter. `patient.id` adalah id kunjungan
itu sendiri — sama seperti yang sudah dipakai `EmergencyTriageHistoryPanel` di layar yang sama.
Karena itu `EmergencyTriageDoctorSection` kini menerima `emergencyVisitId`, bukan `encounterId`.

## 2. Berkas yang disentuh

| Berkas | Perubahan |
| --- | --- |
| `state/slice/.../emergency-management-triage-slice.jsx` | `PATIENT_ENCOUNTER_DOCTOR_URL` dan `assignEmergencyEncounterDoctor` **dicabut**; diganti `EMERGENCY_DOCTOR_ASSIGNMENT_URL` beserta tiga thunk kontrak; bentuk state `doctorAssignment` diganti |
| `hooks/.../use-emergency-triage-doctor.jsx` | Memakai `emergencyVisitId`; memuat riwayat; memilih jalur tetapkan atau alihkan dari keadaan data; memuat ulang riwayat sesudah berhasil |
| `components/.../emergency-triage-doctor-section.jsx` | Daftar riwayat, penanda baris berjalan, isian alasan pengalihan, nama dari backend |
| `components/.../emergency-triage-form-view.jsx` | Meneruskan `emergencyVisitId={patient?.id}` |
| `style/.../emergency-triage.module.css` | Tujuh kelas riwayat dokter |

**Nol perubahan backend, nol perubahan kontrak, nol sentuhan pada `BE-IGD-048`.**

## 3. Tabel keputusan base component

`UI GATE: NO NEW COMPONENT` — seluruh elemen `REUSE` atau `COMPOSE`, jadi tidak ada yang
menunggu keputusan pemilik.

| Elemen | Status | Bukti |
| --- | :-: | --- |
| Pemilih dokter | `REUSE` | `Form.Select` react-bootstrap, sudah dipakai bagian ini sebelumnya |
| Tombol tetapkan/alihkan | `REUSE` | `Button` react-bootstrap, sudah dipakai bagian ini sebelumnya |
| Isian alasan pengalihan | `REUSE` | `Form.Group` + `Form.Control as="textarea"`, pola yang sama dengan bagian lain layar triase |
| Pesan galat dan kosong | `REUSE` | `styles.doctorError`, `styles.doctorEmpty` yang sudah ada |
| Daftar riwayat | `COMPOSE` | `<ol>` bergaya modul, mengikuti pola `emergency-triage-history-timeline.jsx` di modul yang sama. Bukan komponen baru |

## 4. Acceptance criteria

| # | Kriteria | Status | Bukti |
| ---: | --- | :-: | --- |
| 1 | Layar IGD tidak lagi bergantung pada `PATCH /patient-encounters/{id}/doctor` | ✅ | Penelusuran `src/`: **nol** kemunculan di kode; yang tersisa hanya komentar penjelas |
| 2 | Penetapan pertama memakai `POST /` | ✅ | `assignEmergencyDoctor` — slice baris 638 |
| 3 | Pengalihan memakai `POST /{id}/handover`, alasan wajib di layar | ✅ | `handoverEmergencyDoctor` — slice baris 675; penjaga alasan baris 670 dan komponen baris 205 |
| 4 | Riwayat urut waktu: dokter, sejak, sampai, alasan; baris aktif dibedakan | ✅ | `DaftarRiwayatDokter`; `styles.doctorHistoryActive` + badge "Sedang berjalan" |
| 5 | Penolakan `409` tampil beserta arahan memakai aksi pengalihan | ✅ | `assignError` ditampilkan apa adanya; pesan backend sudah memuat *"Gunakan aksi pengalihan dokter."* |
| 6 | Dokter dan penugas tampil sebagai **nama**, bukan ID | ✅ | `tampilkanNama(item?.doctorName)` dan `tampilkanNama(item?.assignedByName)`; nama kosong menjadi tanda hubung. **Nol** panggilan nama per baris |
| 7 | Baris berjalan dikenali dari `effectiveTo` kosong, bukan `isActive` | ✅ | Hook baris 57 `!item?.effectiveTo`; komponen baris 58. **Nol** pembacaan `isActive` sebagai status penugasan |

## 5. Validasi

| Jenis | Hasil |
| --- | --- |
| `npm run lint:errors` | ✅ **PASS**, exit 0 |
| `AUTOMATED TEST: npm test` | ✅ **PASS** — 866 lulus, 0 gagal |
| `npm run build` | ✅ **PASS 18 September 2026 pada revisi `3213419a7`**, dijalankan pemilik. **Bukan Build Verified untuk revisi terbaru** (perubahan bagian 8: terminologi dan fallback "Data historis"): build penuh revisi itu **belum diverifikasi pemilik** |
| `MANUAL TEST` | ✅ **PASS 18 September 2026** — 13 pemeriksaan lewat layar oleh pemilik, [evidence](../evidence/2026-09-18-verifikasi-runtime-fe-igd-027.md). Satu skenario **NOT FEASIBLE**: tampilan baris legacy tanpa pelaku, karena **nol** data yang cocok pada dev (kandidat backfill 0 baris). Bukan `FAIL` |

### 5.1 Checklist konsistensi UI — grep anti-regresi

| Pemeriksaan | Hasil |
| --- | --- |
| `<button>` mentah atau kelas `btn-*` pada JSX baru | **0** ✅ |
| `<table>` tanpa `data-flat-table` | **0** ✅ |
| Utility `fw-*` / `fs-*` bootstrap | **0** ✅ |
| `!important` pada CSS baru | **0** ✅ |
| Literal warna pada CSS baru | **8** ⚠️ lihat 5.2 |
| Deklarasi typography pada CSS baru | **8** ⚠️ lihat 5.2 |

### 5.2 Delta yang dilaporkan apa adanya

Dua pemeriksaan terakhir tidak bersih. Alasannya: berkas
`emergency-triage.module.css` memang menulis nilai visual literal — **234 literal warna**
berbanding **32** pemakaian `var(--…)` pada berkas yang sama.

Menulis blok baru memakai design token akan membuat satu berkas punya dua konvensi yang
bertabrakan. Yang dipilih: **mengikuti konvensi berkas yang sudah ada**, dan mencatat
penyelarasan ke token sebagai pekerjaan tersendiri untuk **seluruh berkas** — bukan
menyelundupkannya lewat task ini. Blok CSS barunya diberi komentar yang menyatakan hal ini.

## 6. Yang **tidak** dikerjakan

| Hal | Alasan |
| --- | --- |
| `BE-IGD-048` | Dilarang disentuh pada task ini. *Keadaan 18 September: ⛔ menunggu `IGD-OQ-092`. Diperbarui 21 September 2026: ✅ — `IGD-OQ-092` ditutup `IGD-DEC-136`, migration diterapkan ke dev dan terbukti di salinan basis data terpisah ([laporan](../backend/BE-IGD-048.md))* |
| Penyelarasan CSS modul ke design token | Pekerjaan satu berkas penuh, di luar lingkup |
| Endpoint Registrasi `PATCH /patient-encounters/{id}/doctor` | **Tidak dihapus** — tetap ada dan tetap milik Registration Management, sesuai API bagian 3. Yang berhenti hanyalah pemakaiannya oleh layar IGD |

## 7. Risiko yang tersisa

| Risiko | Keadaan |
| --- | --- |
| Riwayat kosong pada kunjungan lama | Tabel `EmgDoctorAssignment` belum diisi data lama (`BE-IGD-048` ✅ — migration diterapkan 21 September 2026; dev tetap tanpa baris legacy karena 0 kandidat). Kunjungan lama menampilkan "Belum ada dokter penanggung jawab", dan penetapan berikutnya berjalan sebagai penetapan pertama — bukan pengalihan |
| Celah berhenti membesar | Sejak layar ini memakai kontrak baru, setiap penetapan dokter menghasilkan baris riwayat. Himpunan yang perlu diisi `BE-IGD-048` tidak lagi bertambah |
| Hak akses belum dicentang | `EmergencyDoctorAssignment` Read/Create/Update wajib dicentang admin, kalau tidak seluruh bagian ini gagal `403` |

## 8. Tambahan 18 September 2026 sesudah verifikasi pemilik

### 8.1 Terminologi diperketat

Banner menulis *"dokter pemeriksa"* sementara section menulis *"Dokter Penanggung Jawab"*.
Keduanya diselaraskan menjadi **"Dokter Penanggung Jawab IGD"** atas permintaan pemilik, supaya
tidak rancu dengan DPJP Rawat Inap. Perubahan ini **hanya kata**; nol perubahan perilaku, nol
perluasan lingkup ke domain Rawat Inap.

Batas domainnya ikut ditulis sebagai komentar pada komponen: penugasan IGD berganti mengikuti
shift dan **tidak** diteruskan otomatis menjadi DPJP Rawat Inap.

### 8.2 Tampilan aman untuk baris hasil migrasi data lama

`IGD-DEC-136` mengizinkan `assignedByUserId` kosong pada baris legacy. Layar kini membedakan
dua keadaan yang sebelumnya sama-sama menjadi tanda hubung:

| Keadaan | Tampilan |
| --- | --- |
| `assignedByUserId` kosong atau GUID nol | **"Data historis"** |
| `assignedByUserId` ada tetapi namanya kosong | `-` |

Nol GUID, nol `null` mentah, dan nol `00000000-0000-0000-0000-000000000000` yang tampil ke
petugas. Jalur ini **belum dapat diuji lewat layar** karena datanya belum ada; ia menunggu
`BE-IGD-048`. *Diperluas 21 September 2026 (8.4): baris historis juga tidak lagi menampilkan
`assignmentReason` sebagai "Alasan pengalihan".*

### 8.3 Validasi ulang sesudah dua perubahan di atas

Basis kode bergerak di luar task ini: HEAD berpindah ke `ca31c09c9` dan jumlah unit test naik
dari 866 menjadi 1329 karena pekerjaan modul lain ikut masuk.

| Jenis | Hasil |
| --- | --- |
| `npx eslint --quiet` pada **dua berkas yang disentuh task ini** | ✅ **PASS**, exit 0 |
| Unit test **berkas IGD** (4 berkas `*emergency*`) | ✅ **PASS** — 38 lulus, 0 gagal |
| `npm run lint:errors` **seluruh repo** | ❌ **4 error** — seluruhnya di `inpatient-management/nursing-workspace/.../clinical-instrument-form-renderer.jsx` (`BaseCheckboxCard`, `BaseTextField`, `BaseFormControl` tidak terdefinisi) |
| `npm test` **seluruh repo** | ❌ **9 gagal** dari 1329 — seluruhnya `FE-RWI-*` pada `inpatient-medical-assessment`, `inpatient-physician-entry`, `inpatient-physician-workspace` |

**Kegagalan itu bukan milik `FE-IGD-027`.** Buktinya: **nol** berkas test menyebut
`emergency-triage-doctor-section` maupun `emergency-triage-form-view`; kesembilan kegagalan ada
pada berkas test `inpatient-*`; dan berkas yang memicu lint error berasal dari commit
`8143874d8` milik modul Rawat Inap, bukan dari task ini.

**Dilaporkan apa adanya, bukan diperbaiki** — modul Rawat Inap bukan lingkup task ini, dan
memperbaikinya tanpa wewenang justru menyentuh pekerjaan tim lain.

### 8.4 Koreksi 21 September 2026 — marker teknis tidak tampil sebagai "Alasan pengalihan"

Ditugaskan pemilik pada review final `BE-IGD-048`. Baris hasil `BE-IGD-048` menyimpan penanda
teknis `Data historis - pengisian BE-IGD-048` pada `assignmentReason`; penanda itu dipakai
`Down()` migration dan **tidak diubah**. Sebelum koreksi, layar menampilkannya sebagai
*"Alasan pengalihan: Data historis - pengisian BE-IGD-048"* — keliru, karena baris legacy bukan
hasil pengalihan.

| Baris | Tampilan sesudah koreksi |
| --- | --- |
| Historis (`assignedByUserId` kosong atau GUID nol) | `Ditetapkan Data historis` pada baris meta, lalu baris terpisah **`Sumber: Data historis`**. `assignmentReason` **tidak ditampilkan** |
| Penetapan biasa (ada pelaku, tanpa alasan) | Tidak berubah |
| Handover biasa (ada pelaku, ada alasan) | Tidak berubah — `Alasan pengalihan: <alasan>` tetap tampil |

Satu berkas berubah: `emergency-triage-doctor-section.jsx` (25 tambah, 8 hapus). Nol perubahan pada
hook, slice, CSS, komponen bersama, backend, atau marker basis data. Kriteria baris historis dipakai
bersama oleh `tampilkanPenugas` dan penentu baris "Sumber", lewat satu fungsi `penugasanHistoris`.

| Validasi | Hasil |
| --- | --- |
| `npx eslint` dan `npx eslint --quiet` pada berkas itu | ✅ **PASS**, exit 0, nol keluaran |
| `AUTOMATED TEST`: empat berkas `tests/unit/*emergency*` | ✅ **PASS** — 38 lulus, 0 gagal. Tidak ada test yang menyebut komponen ini; test baru tidak ditulis (komposisi view, opsional menurut `test-policy`) |
| Grep anti-regresi pada baris yang ditambah | ✅ `<button>`/`btn-*` 0, `<table>` 0, `fw-`/`fs-` 0, `!important` 0, literal warna 0, `style=` 0 |
| `MANUAL TEST` — render statis `DaftarRiwayatDokter` (`react-dom/server`, komponen ditransform dari source, hook/CSS/format tanggal di-stub) atas lima skenario: legacy `null`, legacy GUID nol, penetapan biasa, handover biasa, riwayat campuran | ✅ **PASS** — marker `BE-IGD-048` tidak muncul pada satu pun; `Sumber: Data historis` muncul tepat sebanyak baris historis; `Alasan pengalihan: …` hanya pada baris ber-pelaku dan isinya utuh. **Bukan uji browser** |
| `MANUAL TEST` lewat layar | **NOT FEASIBLE** — dev tidak punya baris legacy: migration `BE-IGD-048` sudah dijalankan (21 September 2026), tetapi dev **0 kandidat**, jadi 0 baris tersisip. Baris legacy hanya ada pada salinan basis data terpisah yang sudah dihapus |
| `npm run build` | **NOT RUN** — dipegang pemilik. Build penuh revisi ini **belum diverifikasi** |

**Status `FE-IGD-027` tetap 🟡** — perubahan ini menambah satu revisi lagi yang belum dibangun pemilik. *(Pembaruan 21 September 2026 sore: pemilik menjalankan `npm run build` pada revisi ini dan lulus — status kini ✅, lihat baris `Status` di atas.)*

**Catatan.** Baris historis menampilkan `Sejak <waktu kedatangan pasien>`: `effectiveFrom` hasil
`BE-IGD-048` adalah *historical fallback*, bukan waktu penetapan dokter yang terbukti. Label
"Sejak" tidak diubah (di luar arahan pemilik); kata "Sumber: Data historis" yang menandainya.
