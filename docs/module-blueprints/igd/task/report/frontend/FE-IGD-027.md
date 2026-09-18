# `FE-IGD-027` — Layar triase memakai riwayat penugasan dokter

| Field | Nilai |
| --- | --- |
| Task | `FE-IGD-027` |
| Gelombang | `EPIC IGD-04` · slice `IGD-S06` |
| Status | 🟡 **IMPLEMENTATION COMPLETE / RUNTIME NOT VERIFIED — 17 September 2026.** Ketujuh acceptance terpetakan ke source; lint `PASS`; unit test **866/866**. `npm run build` dan uji lewat layar **belum** — keduanya milik pemilik |
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
| `npm run build` | **Belum dijalankan** — milik pemilik |
| `MANUAL TEST` | **NOT FEASIBLE** bagi agent — menuntut kredensial petugas, backend berjalan, dan hak akses `EmergencyDoctorAssignment` yang dicentang admin |

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
| `BE-IGD-048` | Dilarang disentuh pada task ini; tetap ⛔ menunggu `IGD-OQ-092` |
| Penyelarasan CSS modul ke design token | Pekerjaan satu berkas penuh, di luar lingkup |
| Endpoint Registrasi `PATCH /patient-encounters/{id}/doctor` | **Tidak dihapus** — tetap ada dan tetap milik Registration Management, sesuai API bagian 3. Yang berhenti hanyalah pemakaiannya oleh layar IGD |

## 7. Risiko yang tersisa

| Risiko | Keadaan |
| --- | --- |
| Riwayat kosong pada kunjungan lama | Tabel `EmgDoctorAssignment` belum diisi data lama (`BE-IGD-048` ⛔). Kunjungan lama menampilkan "Belum ada dokter penanggung jawab", dan penetapan berikutnya berjalan sebagai penetapan pertama — bukan pengalihan |
| Celah berhenti membesar | Sejak layar ini memakai kontrak baru, setiap penetapan dokter menghasilkan baris riwayat. Himpunan yang perlu diisi `BE-IGD-048` tidak lagi bertambah |
| Hak akses belum dicentang | `EmergencyDoctorAssignment` Read/Create/Update wajib dicentang admin, kalau tidak seluruh bagian ini gagal `403` |
