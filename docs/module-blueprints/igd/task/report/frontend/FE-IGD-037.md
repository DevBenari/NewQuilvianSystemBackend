# Laporan Perubahan Frontend — `FE-IGD-037`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-037` |
| Judul | Pemilih Dokter Penanggung Jawab IGD menawarkan dokter jaga, dengan override beralasan |
| Slice | `S7` · `EPIC IGD-12` · `MVP-10` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) — kartu di bagian R3.12, konteks perencanaan R3.15 |
| Trace | `FR-IGD-106`, `108` (sisi layar); `AT-IGD-214`, `215`, `216`, `219`; `IGD-DEC-141`, `237`, `239`, `240`; fakta `IGD-FACT-089` |
| Contract version | API **`0.16.0`** §11.2–11.4; validation **`0.15.0`** §13; `03-frontend-architecture.md` §16 — `approved` (`IGD-DEC-240`, 11 Oktober 2026) |
| Wewenang UI | Tanpa layar, komponen dasar, atau CSS global baru. Penandaan kelompok pada `FilterSelect` dan letak isian alasan `DEV_DISCRETION` (`03-frontend-architecture.md` §16.5) |
| Dependency | `BE-IGD-056` 🟡 — build pemilik berhasil dan migration `20261011052215` diterapkan ke dev, 11 Oktober 2026. **Dirilis bersama** |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 1, database 0, keamanan 0, UI/workflow 1 |
| Task mode | `FRONTEND` — izin pemilik 11 Oktober 2026: *"yaa lanjutkan saya beri izin dan anda kerjakan"*; laporan ini ditulis lewat wewenang sempit lintas repository untuk laporan dan tautan bukti |
| Target tulis | `QuilvianSystemFrontendDev` branch `RizkiV2`: enam berkas pada §3.2, ditambah `emergency-triage-history-panel.jsx` dan `emergency-assessment-detail-view.jsx` untuk delta `IGD-DEC-256` (§10). Backend: laporan ini, baris status roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `3335f4b53` (sejajar `origin/RizkiV2`) |
| Commit backend yang dijadikan rujukan | `c7e523fd` + working tree `BE-IGD-056` (belum di-commit) |
| Tanggal | 11 Oktober 2026 |
| Status | ✅ **SELESAI — 11 Oktober 2026 (malam): 8 dari 8 acceptance terbukti** (`build-module-frontend`). Acceptance 7 terbukti lewat R-16 dengan daftar tiruan (`IGD-DEC-263`, §12); penyimpangan pencatatan R-16 disahkan (`IGD-DEC-265`). Validasi delta §10: `eslint` 0 error (1 `EXISTING WARNING`), unit 11/11 (uji IGD 115/115), `npm run build` lulus 489/489. Butir DoD "rilis bersama `BE-IGD-056`" **dilepas dari DoD task atas keputusan pemilik 11 Oktober 2026** (`IGD-DEC-265`, pola `IGD-DEC-264`) dan tetap syarat rilis R3.17.3. *Sebelumnya, malam yang sama:* 🟡 **SEBAGIAN — 7 dari 8 acceptance terbukti** (1–6, 8) sesudah uji ulang Antigravity R-08…R-13 pada bukti mentah dan log backend (§11); penyimpangan penguji disahkan (`IGD-DEC-262`). **Belum:** 7 (roster kosong) — R-14/R-15 `NOT RUN`; ditutup lewat uji layar dengan daftar tiruan R-16 (`IGD-DEC-263`). *Sebelumnya, malam yang sama:* 3 dari 8 (2, 3, 8). Delta `IGD-DEC-256` — penanda override pada DPJP aktif di kedua pintu — **dikerjakan 11 Oktober 2026 malam** (§10): `eslint` 0 error, unit 11/11 (uji IGD 115/115), `npm run build` lulus 489/489. **Belum:** 1 (cabang gagal), 4 (batas 500 karakter di layar), 5, 6 (uji layar penanda), 7 (`AT-IGD-219` pada jendela tanpa jadwal, `IGD-DEC-257`) — seluruhnya di [panduan uji ulang](../../../testing/2026-10-11-panduan-uji-ulang-dpjp-dokter-jaga.md) R-08…R-15. *Sebelumnya, hari yang sama:* 3 dari 8 sesudah putaran Antigravity (§9); lalu implementasi selesai, 2 dari 8 (2, 8) |

---

## 1. Keadaan yang ditemukan di awal

Pemilih Dokter Penanggung Jawab (DPJP) IGD hari ini (`905aec3bd`, `IGD-FACT-089`):

| Temuan | Bukti | Akibat |
| --- | --- | --- |
| Bila `GET …/on-duty-doctors` kosong atau galat, thunk mengambil **seluruh master dokter** (`doctors/options`, 50 baris) | `emergency-management-triage-slice.jsx` `:718` | Penjaga backend dilewati; dokter poli mana pun tampil |
| Layar menyaring spesialisasi sendiri (`isEmergencyDutyDoctor`); bila semua tersaring, **semua** kandidat ditampilkan | `emergency-doctor-option-utils.js` `:18`, `:97` | Kelayakan dihitung di dua tempat yang bisa berbeda pendapat |
| Dokter jaga dan dokter di luar jadwal tidak dibedakan; tidak ada isian alasan override | `emergency-triage-doctor-section.jsx`, `emergency-triage-doctor-modal-dialog.jsx` | Sesudah `BE-IGD-056`, memilih dokter di luar jadwal selalu ditolak `400` tanpa jalan mengisi alasan |
| Tidak ada layar yang menampilkan riwayat penugasan | `DaftarRiwayatDokter` hanya ada di `EmergencyTriageDoctorSection`, yang tidak dipasang di layar mana pun sejak `a8044eaf9` (koreksi §2) | Penanda `isRosterOverride` dari backend tidak terbaca di layar |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** perawat triage / perawat IGD. **Kapan:** sesudah penilaian triage tersimpan, lewat salah satu dari **dua
pintu**. Keduanya membuka modal yang sama, *Tetapkan/Alihkan Dokter Penanggung Jawab IGD*
(`emergency-triage-doctor-modal-dialog.jsx`):

| Pintu | Layar dan tombol | Berkas yang memasang modal |
| --- | --- | --- |
| A | *Riwayat Triage Pasien* — *Tetapkan Dokter Sekarang* atau *Alihkan Dokter* pada banner DPJP | `emergency-triage-history-panel.jsx` |
| B | *Ruang Kerja Pemeriksaan IGD* — *+ Tetapkan Dokter PJ* atau *DPJP: (nama dokter)* | `emergency-assessment-detail-view.jsx` |

> **Koreksi 11 Oktober 2026.** Versi awal laporan ini menyebut bagian *Dokter Penanggung Jawab IGD* pada Detail triage
> sebagai pintu. Bagian itu — `EmergencyTriageDoctorSection`, termasuk `DaftarRiwayatDokter` dan tanda *"Di luar jadwal
> jaga — alasan"* — **tidak dipasang di layar mana pun** sejak commit `a8044eaf9` (7 Oktober 2026). Perubahan task ini
> pada berkas itu ada di source, tetapi tidak terlihat pengguna.

1. Perawat membuka pemilih. Layar meminta daftar ke backend.
2. Daftar tampil dalam dua kelompok: **Dokter jaga IGD saat ini**, lalu **Di luar jadwal jaga — wajib alasan**. Label
   setiap dokter diambil dari backend (`displayLabel`).
3. Perawat memilih dokter jaga → tombol aktif → tersimpan.
4. Perawat memilih dokter di luar jadwal → isian *Alasan memilih dokter di luar jadwal jaga* muncul. Tombol baru aktif
   setelah alasan terisi (paling banyak 500 karakter). Pada pengalihan, isian ini terpisah dari *Alasan pengalihan*.
5. Sesudah tersimpan, banner DPJP (pintu A) dan tombol *DPJP: (nama dokter)* (pintu B) menampilkan dokter baru. Penanda override
   dan alasannya tersimpan di backend dan terbaca lewat `GET …?emergencyVisitId=` (`isRosterOverride`,
   `rosterOverrideReason`), tetapi **belum tampil di layar** — lihat acceptance 6.

*Contoh:* pukul 10.00 perawat Dimas membuka modal pengalihan untuk Tn. Budi. Di bawah judul *Dokter jaga IGD saat ini*
ada dr. Ranger (Shift Pagi IGD - 07:00 - 14:00). Di bawah *Di luar jadwal jaga — wajib alasan* ada dr. Andi — Umum (di
luar jadwal jaga). Ia memilih dr. Andi, menulis alasan pengalihan dan alasan memilih dokter di luar jadwal, lalu menekan
*Alihkan Dokter*.

**Jalur tidak normal.**

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Tidak ada dokter berjadwal pada jam itu | Keterangan *"Tidak ada dokter berjadwal jaga IGD pada jam ini. Pilih dokter di luar jadwal jaga dan isi alasannya."*; kelompok kedua tetap dapat dipilih |
| Backend menolak (`400` validation §13 aturan 8–10), misalnya roster berubah sesudah daftar dibuka | Pesan backend tampil apa adanya di tempat aksi; daftar dimuat ulang |
| Daftar gagal dimuat | Pesan galat tampil; **tidak** ada daftar cadangan dari sumber lain |
| Tidak ada dokter aktif sama sekali | Pesan lama *"Belum ada dokter aktif pada master dokter."* |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/` (`base-component-decision-gate`, `design-tokens`, `test-policy`,
`ui-consistency-checklist`, `REPORT_TEMPLATE`); kartu `FE-IGD-037`; kontrak API §11, validation §13,
`03-frontend-architecture.md` §16; enam berkas pada §3.2; `filter-select.jsx` dan `filter-select.module.css` (dukungan
opsi `disabled`, prop `renderOption`); `emergency-triage.module.css` (kelas `doctor*`, `dialogFieldRequired`);
pemakai hook `use-emergency-triage-doctor` (`emergency-triage-history-panel.jsx`, `emergency-assessment-detail-view.jsx`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/health-services/emergency-installation-management/emergency-management-triage-slice.jsx` | `fetchEmergencyDoctorOptions` hanya memanggil `on-duty-doctors`; cadangan ke master dokter dan konstanta `DOCTOR_OPTIONS_URL` dihapus. `assignEmergencyDoctor` dan `handoverEmergencyDoctor` mengirim `rosterOverrideReason` bila terisi |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-management-triage/use-emergency-triage-doctor.jsx` | Status `overrideReason` (dikosongkan saat dokter berganti); `isOverrideSelected` dari `isOnDuty` dokter terpilih; `overrideReasonMissing`; `hasOnDutyDoctors`; daftar dimuat ulang sesudah penyimpanan gagal |
| `src/utils/health-services/emergency-installation-management/emergency-doctor-option-utils.js` | `isEmergencyDutyDoctor` dan saringan `filtered.length > 0 ? filtered : items` dihapus; `EMERGENCY_DOCTOR_GROUP_LABELS`, `groupEmergencyDoctorOptions`, `buildEmergencyDoctorFilterOptions` (judul kelompok berupa opsi nonaktif) |
| `src/components/view/health-services/emergency-installation-management/emergency-management-triage-view/components/emergency-triage-doctor-section.jsx` | Dua `<optgroup>`; isian alasan override; keterangan roster kosong; penanda override pada `DaftarRiwayatDokter`. **Tidak terlihat pengguna** — komponen ini tidak dipasang sejak `a8044eaf9` (koreksi §2) |
| `src/components/view/health-services/emergency-installation-management/emergency-management-triage-view/components/emergency-triage-doctor-modal-dialog.jsx` | `FilterSelect` dengan judul kelompok; isian alasan override; keterangan roster kosong |
| `tests/unit/emergency-doctor-shift-assignment.test.mjs` | Tes *"strictly filters out outpatient specialists"* diganti — perilakunya sengaja dihapus; empat tes baru untuk pemetaan tanpa saringan, pengelompokan, dan judul kelompok |

`git diff --stat` enam berkas: 6 files changed, 277 insertions(+), 97 deletions(-).

### 3.3 Kepatuhan arsitektur frontend

Alur tetap view → hook → thunk Redux → `InstanceAxios`; logika murni di `src/utils`. Nol komponen, route, slice, atau
service baru. Tabel keputusan base component:

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Dua kelompok di `EmergencyTriageDoctorSection` (tidak dipasang — koreksi §2) | `Form.Select` + `<optgroup>` | sudah dipakai `emergency-triage-doctor-section.jsx` | REUSE | `optgroup` bawaan HTML |
| Dua kelompok di modal (pintu A dan B) | `FilterSelect` opsi `disabled` | `filter-select.jsx` (`option.disabled`, `.optionDisabled`) | REUSE | Judul kelompok sebagai opsi nonaktif |
| Isian alasan di luar jadwal | `Form.Control as="textarea"` | pola alasan pengalihan di kedua pintu | REUSE | `maxLength={500}` |
| Penanda di riwayat (`DaftarRiwayatDokter`, tidak dipasang) | kelas `doctorHistoryReason` | `emergency-triage.module.css` | REUSE | Satu baris teks |
| Keterangan roster kosong, galat, tanda wajib | kelas `doctorEmpty`, `doctorError`, `dialogFieldRequired` | berkas CSS yang sama | REUSE | — |

`UI GATE: 5 elemen — REUSE 5, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Pilihan berbunyi *"Memuat dokter..."* / *"Memuat daftar dokter..."*; kontrol nonaktif (perilaku lama) |
| Kosong | Roster kosong: keterangan pada §2. Tanpa dokter aktif sama sekali: *"Belum ada dokter aktif pada master dokter."* |
| Gagal | Pesan galat backend, atau *"Gagal mengambil daftar dokter jaga IGD."*; penolakan simpan tampil apa adanya dan daftar dimuat ulang |
| Tanpa hak akses | `403` dari backend tampil sebagai pesan galat (perilaku lama `normalizeErrorMessage`) |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Doctor Assignment

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-doctor-assignments/on-duty-doctors` | Daftar dua kelompok | `EmergencyDoctorAssignment : Read` |
| `POST` | `/v1/health-services/emergency-installation-management/emergency-doctor-assignments` | Penetapan pertama, dengan `rosterOverrideReason` bila di luar jadwal | `EmergencyDoctorAssignment : Create` |
| `POST` | `/v1/health-services/emergency-installation-management/emergency-doctor-assignments/{id}/handover` | Pengalihan, dengan `rosterOverrideReason` bila di luar jadwal | `EmergencyDoctorAssignment : Update` |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-doctor-assignments?emergencyVisitId=` | Riwayat, membaca `isRosterOverride`, `rosterOverrideReason` | `EmergencyDoctorAssignment : Read` |

`GET /v1/corporate/human-resource/master-data/doctors/options` **tidak lagi** dipanggil dari pemilih ini.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` enam berkas task | Keluar 0 — nol error, nol warning | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/emergency-doctor-shift-assignment.test.mjs` | 10/10 lulus | `PASS` | Keluaran perintah |
| Regresi uji IGD `tests/unit/emergency-*.test.mjs` | 114/114 lulus | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil — `✓ Compiled successfully in 2.3min`, halaman statis 489/489, nol baris warning pada log; selesai sekitar 15.44 WIB, 11 Oktober 2026. Build ini juga memuat perubahan pendaftaran milik pemilik di working tree | `PASS` | `scratchpad` log build |
| Grep anti-regresi (warna literal, tipografi, `<button>` mentah, `!important`, komentar baru) pada enam berkas | Kosong | `PASS` | `git diff -U0` |
| `git diff --check` enam berkas | Bersih | `PASS` | Keluaran perintah |
| Akhiran baris | Enam berkas tetap CRLF tanpa BOM | `PASS` | Hitungan byte lewat Node |
| Uji layar `AT-IGD-214`, `215`, `216`, `219` — putaran Antigravity 11 Oktober 2026, build produksi, akun perawat IGD | 214 dan 215 lulus; 216 sebagian (isian alasan dan penyimpanan lulus, penanda tidak tampil di layar); 219 tidak dijalankan (tidak ada jam tanpa jadwal) | `PASS` / `PARTIAL` / `NOT RUN` | §9 |

AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/emergency-*.test.mjs — PASS (114/114). `npm run test:unit` tidak dipakai karena gagal sebelum test jalan di Node 24 (argumen folder) — `EXISTING / ENVIRONMENT ISSUE`.

MANUAL TEST: dijalankan agen Antigravity atas nama pemilik pada hasil build dengan akun perawat IGD; agent memutus dari bukti mentah dan log backend (§9).

**Tidak dijalankan:** cabang gagal daftar dokter (acceptance 1), penolakan `400` dari layar (acceptance 5), roster kosong (acceptance 7).

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Daftar hanya dari `GET …/on-duty-doctors`; bila gagal, pesan galat tampil dan tidak ada daftar dari sumber lain | **Terpenuhi** — jalur berhasil: daftar dari `on-duty-doctors`, nol `doctors/options` di kelima sesi layar putaran pertama. Cabang gagal (R-12): respons `500` tiruan → modal menampilkan *"Uji: daftar dokter gagal dimuat"*, tidak ada dokter yang dapat dipilih, tombol simpan nonaktif, nol permintaan ke master dokter (jaringan dan log backend) | `AT214-pintuA-network.json`, `AT214-pintuB-network.json`, log backend 16.33–16.36 WIB; `R-12.png`, `R-12-network.json` (§11) |
| 2 | Layar tidak menyaring spesialisasi; `isEmergencyDutyDoctor` dan saringan `:97` hilang | **Terpenuhi** | Diff util |
| 3 | Kedua pintu — modal di panel riwayat triage (pintu A) dan modal di Ruang Kerja Pemeriksaan IGD (pintu B), koreksi §2 — menampilkan dua kelompok dengan judul berbeda dan memakai `displayLabel` | **Terpenuhi** | `AT214-pintuA.png`, `AT214-pintuB.png`: judul *Dokter jaga IGD saat ini* dan *Di luar jadwal jaga — wajib alasan*; label sama persis dengan `displayLabel` di `AT214-api.json`; D3 tidak tampil. Source: `buildEmergencyDoctorFilterOptions` di modal |
| 4 | Dokter `isOnDuty=false` memunculkan isian alasan wajib (1–500); tombol nonaktif sampai terisi; terpisah dari alasan pengalihan | **Terpenuhi** — isian muncul, tombol nonaktif dan pesan wajib tampil saat kosong, dua isian terpisah pada pengalihan (DOM `AT-IGD-216`); 501 huruf menjadi 500, `maxlength="500"`, nol `POST` (R-11) | `AT216-*-network.json`; `R-11-kosong.png`, `R-11-500.png`, `R-11-network.json` (§11) |
| 5 | Penolakan `400` aturan 8–10 tampil apa adanya; daftar dimuat ulang | **Terpenuhi** — `POST` yang diubah ke D3 ditolak `400` aturan 8; modal tetap terbuka dan menampilkan pesan itu apa adanya; satu `GET …/on-duty-doctors` baru sesudah penolakan; riwayat V3 tetap nol baris (R-13 percobaan 3) | `R-13.png`, `R-13-network.json`, `R-13-api.json`; log 21.17.35–21.17.36 WIB (§11) |
| 6 | Riwayat menampilkan penanda *Di luar jadwal jaga* dan alasan pada baris `isRosterOverride=true` — **dibaca "penanda pada DPJP aktif"** (`IGD-DEC-256`) | **Terpenuhi** — pintu A dan B menampilkan penanda dan alasan untuk DPJP override (R-08), tidak menampilkan apa pun untuk DPJP dokter jaga (R-09), dan menampilkan alasan hasil pemangkasan server (R-10). *Putaran 11 Oktober sore:* tidak tampil karena `DaftarRiwayatDokter` tidak dipasang sejak `a8044eaf9` | `R-08-*.png`, `R-09-*.png`, `R-10-pintuA.png` dan JSON jaringannya (§11) |
| 7 | Roster kosong: keterangan tampil dan penetapan lewat kelompok di luar jadwal berhasil | **Terpenuhi** — dengan daftar dokter jaga dikosongkan lewat `page.route` (`IGD-DEC-263`): teks keterangan persis ditemukan di DOM modal, hanya judul *Di luar jadwal jaga — wajib alasan* yang tampil, penetapan D2 dengan alasan → `201` override, banner menampilkan penanda, riwayat V3 satu baris (R-16). *Sebelumnya:* `AT-IGD-219` dan R-14/R-15 `NOT RUN` (jadwal IGD dev terisi). Uji dengan jadwal kosong yang nyata tetap cek lingkungan (`IGD-UNK-18`) | `R-16-kosong.png`, `R-16-banner.png`, `R-16-network.json`, `R-16-api.json` (§12) |
| 8 | `eslint` berkas task 0 error; unit test yang disesuaikan lulus; `npm run build` lulus | **Terpenuhi** — eslint 0 error 0 warning; unit 10/10 (IGD 114/114); build 489/489 | §6 |

DoD: laporan ini ada; register, grafik R3.12.1, dan traceability diperbarui. Butir "rilis bersama `BE-IGD-056`" dilepas dari DoD task (`IGD-DEC-265`) dan tetap syarat rilis R3.17.3 — belum dipenuhi.
Riwayat penugasan dan tampilan *"Data historis"* dari `FE-IGD-027` tidak diubah.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Server Next di port 3000 (PID 3704) dihentikan agent atas izin pemilik sebelum build; nyalakan lagi untuk uji. Satu build pertama dihentikan agent karena suntingan kecil (`dialogFieldRequired`) masuk saat build berjalan; dua proses sisa build itu (milik agent) ikut dihentikan, lalu build diulang |
| Masalah yang diketahui | Judul kelompok di modal tampil sebagai opsi nonaktif (abu-abu) — gaya bawaan `FilterSelect`, sesuai `DEV_DISCRETION` §16.5. Parameter `onAssigned.doctorName` di modal tetap memakai label opsi (perilaku lama), sehingga untuk dokter di luar jadwal teksnya ikut berakhiran *"(di luar jadwal jaga)"* |
| Dependency backend | `BE-IGD-056` 🟡 — build dan migration sudah; uji API sebagian (laporan `BE-IGD-056` §8). Rilis wajib bersama |
| Perubahan sampingan | `NONE`. Sembilan berkas pendaftaran IGD yang berubah di working tree adalah pekerjaan pemilik yang sedang berjalan dan tidak disentuh; build di §6 ikut memuatnya |
| Interupsi | Build pertama dihentikan dan diulang (lihat *Peringatan*) |
| Status Git | Enam berkas §3.2 dan dua berkas delta §10 (`M`), ditambah berkas pendaftaran milik pemilik (`M`) |
| Langkah berikutnya | Pemilik: commit; rilis `FE-IGD-037` **bersama** `BE-IGD-056` (keduanya ✅), dengan migration `20261011052215` diterapkan lebih dulu dan `IGD-UNK-18` dicatat untuk lingkungan rilis (R3.17.3). Server port 3000 sudah memakai build yang memuat seluruh task ini (`BUILD_ID` 20.51 WIB) |

---

## 9. Pemeriksaan bukti uji — 11 Oktober 2026

Uji dijalankan agen Antigravity atas nama pemilik, 16.07–16.42 WIB, pada build produksi dengan akun perawat IGD. Agent
memutus dari bukti mentah `QuilvianSystemFrontendDev/test-with-agy/igd/uji-dpjp-dokter-jaga-2026-10-11/` (JSON, lima
PNG, skrip `run-at214.mjs`…`run-at220.mjs`) yang dicocokkan dengan `Logs/quilvian-backend-20261011.json`, bukan dari
ringkasan [laporan penguji](../../../testing/2026-10-11-laporan-uji-antigravity-dpjp-dokter-jaga.md). Sisi backend,
pencocokan log lengkap, dan daftar penyimpangan ada di [laporan `BE-IGD-056` §8](../backend/BE-IGD-056.md).

### 9.1 Aturan uji

| Pemeriksaan | Hasil |
| --- | --- |
| Build produksi | Kelima PNG: `nextjsPortalNull: true` di JSON sesinya; nol piksel gelap di pojok kiri bawah (x14–62, y838–886), jadi tidak ada lencana "N" |
| Akun | `dimas.kurniawan` (perawat IGD), `isSuperAdmin: false` pada respons `auth/permissions`; nol permintaan SuperAdmin sesudah 15.54 WIB |
| Sandi | Skrip membaca email dan sandi dari variabel lingkungan; nol nilai sandi atau token di JSON; `Authorization` dan `Cookie` disamarkan |
| Larangan | Nol SuperAdmin, *Simpan Akses*, `role-access`, atau SQL di skrip. Kecocokan kata kunci hanya komentar "Select" dan data historis (`createByName: SuperAdmin`) |

### 9.2 Putusan sisi layar

| Skenario | Putusan agent | Bukti yang menentukan |
| --- | --- | --- |
| `AT-IGD-214` | **PASS** | PNG pintu A dan B: judul *Dokter jaga IGD saat ini* → dr. Farhan Malik *(Shift Siang IGD - 14:00 - 21:00)*; judul *Di luar jadwal jaga — wajib alasan* → dr. Ranger Biru dan dr. Rendy Pangalila *— Umum (di luar jadwal jaga)*; D3 tidak tampil. Jaringan: `on-duty-doctors?onlyOnDuty=false`, nol `doctors/options` |
| `AT-IGD-215` | **PASS** | Pintu A, kunjungan V1: memilih D1 → isian alasan tidak muncul, tombol aktif; `POST` tanpa `rosterOverrideReason` → `201`; riwayat `isRosterOverride=false`, alasan `null`; banner DPJP dr. Farhan Malik tanpa penanda |
| `AT-IGD-216` | **Sebagian** — isian dan penyimpanan lulus; penanda di layar tidak ada | Pintu B, V2: isian alasan muncul, tombol nonaktif dan pesan wajib tampil saat kosong, aktif sesudah terisi; `POST` `201` membawa alasan. Pintu A, V1: dua isian terpisah, tombol nonaktif sampai keduanya terisi; `POST …/handover` `200`. Riwayat API: `isRosterOverride=true` dan alasan pada kedua kunjungan. PNG sesudah simpan menampilkan dr. Ranger Biru **tanpa** penanda *Di luar jadwal jaga*. Bukan kegagalan skrip: komponen penandanya tidak dipasang (koreksi §2) |
| `AT-IGD-219` | **NOT RUN** | Tidak ada jam tanpa jadwal IGD di dev; jam uji 16.42 berada di Shift Siang dr. Farhan Malik. Klaim penguji "168 jam terisi" bersandar pada pemindaian `?at=` yang hasilnya tidak disimpan |

Alasan berspasi `"  Dua dokter jaga sedang resusitasi  "` sudah dipangkas thunk (`emergency-management-triage-slice.jsx`
`:767`, `:806`) sebelum dikirim; body permintaan di rekaman jaringan tanpa spasi di ujung.

### 9.3 Pencocokan dengan log

Seluruh permintaan di catatan jaringan kelima sesi layar cocok dengan log backend (method, path, kode status, akun):
`AT-IGD-214` pintu A 11/11 dan pintu B 9/9; `AT-IGD-215` 7/7; `AT-IGD-216` pintu B 7/7 dan pintu A 7/7. Tiga penulisan
yang berhasil hari itu — `POST` `201` 16.34.32 dan 16.35.37 WIB, `POST …/handover` `200` 16.35.48 WIB — tercatat sebagai
`UserAction` dan semuanya dilaporkan penguji.

### 9.4 Menunggu putusan pemilik

**Acceptance 6 dan bukti riwayat `AT-IGD-216`.**

| Opsi | Isi | Akibat |
| --- | --- | --- |
| a | Tambahan FE kecil: satu baris *Di luar jadwal jaga — alasan: …* pada banner DPJP pintu A dan pada tampilan DPJP pintu B, bila DPJP aktif ber-`isRosterOverride` benar | Dua berkas di luar enam berkas Cakupan (delta yang perlu disahkan); kriteria 6 dibaca "penanda pada DPJP aktif", bukan daftar riwayat; uji ulang hanya bagian layar `AT-IGD-216` |
| b | Terima `AT216-api.json` sebagai bukti dan catat sebagai delta yang disahkan pemilik | Nol kode; `FR-IGD-108` bagian "terbaca di layar" belum terpenuhi — override hanya terlihat lewat API |
| c | Pasang kembali daftar riwayat penugasan (`DaftarRiwayatDokter`) di modal atau panel | Memenuhi kriteria apa adanya; mengubah tata letak sehingga butuh wewenang UI; paling besar |

Rekomendasi agent: **(a)**. Perawat dan dokter langsung melihat bahwa DPJP yang sedang bertugas dipilih di luar jadwal
beserta alasannya, dan perubahannya hanya tampilan pada data yang sudah dikirim backend. Daftar riwayat lengkap (c) dapat
menjadi task terpisah bila tinjauan override perlu di layar.

**`AT-IGD-219` (acceptance 7).** Agent tidak mengubah jadwal. Pilihannya: pemilik menyiapkan satu jendela tanpa jadwal IGD
di dev lalu uji ulang; atau acceptance 7 dicatat sebagai delta berdasarkan source dan unit test. Rekomendasi agent:
jendela uji, karena keterangan roster kosong adalah jalan keluar saat jadwal belum terisi di sebuah lingkungan
(`IGD-UNK-18`).

**Putusan pemilik, 11 Oktober 2026:** acceptance 6 memakai opsi **(a)** (`IGD-DEC-256`), dikerjakan pada §10;
`AT-IGD-219` diuji ulang pada jendela tanpa jadwal yang disiapkan pemilik (`IGD-DEC-257`); penyimpangan pelaporan
penguji disahkan (`IGD-DEC-255`).

---

## 10. Delta `IGD-DEC-256` — penanda override pada DPJP aktif, 11 Oktober 2026 (malam)

### 10.1 Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/emergency-installation-management/emergency-doctor-option-utils.js` | `buildEmergencyDoctorOverrideNote(assignment)` — fungsi murni: *"Di luar jadwal jaga — alasan: …"* (alasan dipangkas) bila `isRosterOverride` benar; *"Di luar jadwal jaga"* bila alasannya kosong; string kosong selain itu |
| `src/components/view/health-services/emergency-installation-management/emergency-management-triage-view/components/emergency-triage-history-panel.jsx` | **Pintu A.** Banner DPJP aktif menampilkan baris penanda di bawah *Bertugas sejak … • Ditugaskan oleh …* (+6) |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/emergency-assessment-detail-view.jsx` | **Pintu B.** Ruas *Dokter PJ IGD* di kartu *Informasi Pasien* berisi *"(nama dokter) • (penanda)"* lewat prop `assignedDoctorName` yang sudah ada; berkas kartu pasien tidak diubah (+8/−1) |
| `tests/unit/emergency-doctor-shift-assignment.test.mjs` | Satu tes baru untuk `buildEmergencyDoctorOverrideNote` (override dengan alasan berspasi, override tanpa alasan, bukan override, tanpa ruas, `null`) |

Data diambil dari riwayat yang sudah dimuat hook (`activeAssignment.isRosterOverride`, `rosterOverrideReason` dari
`GET …?emergencyVisitId=`); nol thunk, endpoint, atau state baru. Nol komentar baru. *Contoh:* Tn. Budi dialihkan ke dr.
Andi (Umum, tidak berjadwal) dengan alasan *"Dua dokter jaga sedang resusitasi"*. Banner pintu A menampilkan *"Di luar
jadwal jaga — alasan: Dua dokter jaga sedang resusitasi"*, dan ruas *Dokter PJ IGD* pintu B menampilkan *"dr. Andi • Di
luar jadwal jaga — alasan: Dua dokter jaga sedang resusitasi"*. DPJP dokter jaga tidak menampilkan baris apa pun.

### 10.2 Gerbang base component

Modul referensi visual: banner DPJP pintu A dan kartu *Informasi Pasien* pintu B itu sendiri.

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Penanda dan alasan pada banner DPJP pintu A | kelas `doctorHistoryReason` | `emergency-triage.module.css:2551`; dipakai `DaftarRiwayatDokter` untuk teks yang sama | REUSE | Satu `<p>` di dalam `doctorNoticeInfo` |
| Penanda dan alasan pada tampilan DPJP pintu B | ruas *Dokter PJ IGD* `EmergencyAssessmentPatientCard` | prop `assignedDoctorName` (`emergency-assessment-patient-card.jsx:59`, `:100`); `dd` memakai `word-break: break-word`, tidak memotong teks | REUSE | Nilai ruas = nama • penanda |

`UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

Nol CSS baru; nol warna, ukuran huruf, atau `!important` baru.

### 10.3 Validasi

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` empat berkas delta | 0 error, 1 warning `react-hooks/set-state-in-effect` pada `emergency-triage-history-panel.jsx` (`setCurrentPage` di dalam effect) — sudah ada di `HEAD` (baris 387 sebelum delta, diperiksa lewat `git show HEAD:… \| npx eslint --stdin`) | `PASS` / `EXISTING WARNING` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/emergency-doctor-shift-assignment.test.mjs` | 11/11 lulus | `PASS` |
| Regresi `tests/unit/emergency-*.test.mjs` | 115/115 lulus | `PASS` |
| `npm run build` (port 3000 kosong sebelum build) | Berhasil — `✓ Compiled successfully in 3.1min`, halaman statis 489/489, nol baris warning, `prepare-standalone` menyalin static dan public; `.next/BUILD_ID` 11 Oktober 2026 20.51 WIB. Build ini juga memuat perubahan pendaftaran milik pemilik di working tree | `PASS` |
| `git diff --check` empat berkas | Bersih | `PASS` |
| Akhiran baris | Keempat berkas tetap CRLF tanpa BOM | `PASS` |
| Grep anti-regresi pada baris tambahan (komentar, `<button`, `<table`, `fw-`/`fs-`, `!important`, warna literal) | Kosong | `PASS` |

AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/emergency-*.test.mjs — PASS (115/115)

MANUAL TEST: NOT FEASIBLE — server port 3000 tidak berjalan dan uji layar menjadi bagian putaran Antigravity pemilik
(panduan uji ulang R-08…R-10).

---

## 11. Pemeriksaan uji ulang — 11 Oktober 2026 (malam)

Uji ulang dijalankan agen Antigravity atas nama pemilik, 21.07–21.18 WIB, pada server dari `BUILD_ID` 20.51 WIB (memuat
delta §10). Agent memutus dari bukti mentah `QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-dpjp-dokter-jaga-2026-10-11/`
(`percobaan.json`, delapan JSON jaringan, sembilan PNG, sebelas skrip) yang dicocokkan dengan
`Logs/quilvian-backend-20261011.json`, bukan dari ringkasan
[laporan penguji](../../../testing/2026-10-11-laporan-uji-ulang-antigravity-dpjp-dokter-jaga.md). Sisi backend R-01…R-07:
[laporan `BE-IGD-056` §9](../backend/BE-IGD-056.md).

**Aturan uji.** Kesembilan PNG `nextPortalNull: true` dan nol piksel gelap di pojok kiri bawah (tanpa lencana "N"). Akun
`dimas.kurniawan`; nol SuperAdmin, SQL, atau sandi tertulis; `Cookie` disamarkan. `page.route` hanya pada R-12 dan R-13,
persis seperti panduan. Rekaman jaringan merekam semua `/api/v1/` sesudah login.

| Skenario | Putusan agent | Bukti yang menentukan |
| --- | --- | --- |
| R-08 | **PASS** — acceptance 6 | Pintu A V1: baris *"Di luar jadwal jaga — alasan: Dua dokter jaga sedang resusitasi"* di bawah *Bertugas sejak …*. Pintu B V1: ruas *Dokter PJ IGD* *"dr. Ranger Biru • Di luar jadwal jaga — alasan: Dua dokter jaga sedang resusitasi"* (teks membungkus empat baris) |
| R-09 | **PASS** — acceptance 6 | V2 sesudah pengalihan ke dokter jaga (R-06): pintu A banner dr. Farhan Malik tanpa baris penanda; pintu B ruas *"dr. Farhan Malik"* saja |
| R-10 | **PASS** — acceptance 6 | V2 sesudah R-07: baris *"Di luar jadwal jaga — alasan: Uji pemangkasan alasan di server"* |
| R-11 | **PASS** — acceptance 4 | V3 (pasien di layar: ANISA PRAMESTI): memilih D2 memunculkan isian; saat kosong tombol *Tetapkan Dokter* nonaktif dan pesan wajib tampil; 501 huruf menjadi 500, `maxlength="500"`; nol `POST` (log 21.13.08–21.13.13 WIB) |
| R-12 | **PASS** — acceptance 1 | `on-duty-doctors` dijawab `500` tiruan (dua kali; nol permintaan itu di log backend): pesan *"Uji: daftar dokter gagal dimuat"* tampil, pemilih kosong, tombol nonaktif, nol permintaan ke master dokter |
| R-13 | **PASS** — acceptance 5 (percobaan 3) | `POST` 21.17.35 WIB → `400` aturan 8; modal tetap terbuka dengan pesan itu apa adanya; `GET …/on-duty-doctors` 21.17.36 WIB (daftar dimuat ulang); riwayat V3 nol baris |
| R-14, R-15 | **NOT RUN** — acceptance 7 | 21.18.46 WIB dr. Farhan Malik Shift Malam `isOnDuty=true`; jendela tanpa jadwal tidak disiapkan. Diganti R-16 (`IGD-DEC-263`) |

**Pencocokan.** Rekaman jaringan cocok dengan log (method, path, kode status, akun): R-08 pintu A 11 dan pintu B 9, R-09
11 dan 9, R-10 11, R-11 11, R-12 11, R-13 13. Sesi yang diawali login memuat satu `GET /auth/me` tambahan di log sesaat
sesudah login, sebelum perekam dipasang — pola yang sama pada putaran sore. Dua `on-duty-doctors` R-12 dipenuhi
`page.route` dan tidak sampai ke backend.

**Penyimpangan** (`IGD-FACT-107`, disahkan `IGD-DEC-262`): nama pasien V3 di laporan penguji keliru (*"An. Rizky"*;
layar *ANISA PRAMESTI*); percobaan 1 R-13 mengirim `POST` yang ditolak `400` (21.14.48 WIB) tanpa disebut; percobaan 2
R-13 menutup browser sebelum respons sehingga backend mencatat `500` pada kueri baca, sebelum penyimpanan.

**Catatan tampilan.** Pada pintu B, nilai ruas *Dokter PJ IGD* dengan alasan panjang membungkus beberapa baris di kartu
*Informasi Pasien* (R-08). Tidak memotong teks dan tidak menggeser ruas lain; dicatat untuk dinilai pemilik, bukan cacat.

---

## 12. Pemeriksaan R-16 roster kosong — 11 Oktober 2026 (malam)

R-16 dijalankan agen Antigravity atas nama pemilik mengikuti
[panduan uji ulang](../../../testing/2026-10-11-panduan-uji-ulang-dpjp-dokter-jaga.md) §5 (`IGD-DEC-263`). Agent memutus
dari bukti mentah `QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-r16-dpjp-2026-10-11/` (`percobaan.json`,
`R-16-network.json`, `R-16-api.json`, dua PNG, tiga versi skrip) yang dicocokkan dengan
`Logs/quilvian-backend-20261011.json`, bukan dari ringkasan
[laporan penguji](../../../testing/2026-10-11-laporan-uji-r16-dpjp-dokter-jaga.md).

| Langkah | Putusan agent | Bukti yang menentukan |
| --- | --- | --- |
| P0 | **PASS** | Riwayat V3 nol baris sebelum penetapan (rekaman jaringan percobaan 3, `GET …?emergencyVisitId=` → `[]`) |
| 1 | **PASS** | `page.route` persis seperti panduan (`run-r16-p3.mjs` `:175`–`183`); kedua respons `on-duty-doctors` yang sampai ke layar hanya berisi dr. Ranger Biru dan dr. Rendy Pangalila (`isOnDuty=false`). Teks *"Tidak ada dokter berjadwal jaga IGD pada jam ini. Pilih dokter di luar jadwal jaga dan isi alasannya."* ditemukan persis di DOM modal; `R-16-kosong.png` menampilkan judul *Di luar jadwal jaga — wajib alasan* dan kedua dokter, tanpa judul dokter jaga |
| 2 | **PASS** | `POST` V3 + D2, `rosterOverrideReason` *"Uji roster kosong"* → `201`, `isRosterOverride=true` (21.44.43 WIB; satu-satunya penulisan); respons ditunggu sebelum browser ditutup |
| 3 | **PASS** | `R-16-banner.png`: DPJP dr. Ranger Biru, *Bertugas sejak 11 Okt 2026, 21.44*, baris *"Di luar jadwal jaga — alasan: Uji roster kosong"* |
| 4 | **PASS** | `R-16-api.json`: satu baris D2, `isRosterOverride=true`, alasan *"Uji roster kosong"* |

**Lingkungan.** Server port 3000 `node .next/standalone/server.js` PID 6644 sejak 21.06.05 WIB dari `BUILD_ID` 20.51 WIB —
proses yang sama dengan uji ulang R-08…R-13 (§11); dua PNG tanpa lencana "N". Akun `dimas.kurniawan`; nol SuperAdmin atau
SQL; sandi dari variabel lingkungan; `Cookie` disamarkan.

**Penyimpangan** (`IGD-FACT-109`, disahkan `IGD-DEC-265`): (a) percobaan 1 dan 2 tercatat "0 request", padahal log memuat
login dan, pada percobaan 2, satu sesi layar penuh yang hanya membaca; (b) hasil cek `nextjs-portal` hanya dicetak di
konsol skrip; (c) keterangan roster kosong tertutup daftar yang terbuka di `R-16-kosong.png`, karena panduan meminta
tangkapan layar diambil saat pemilih terbuka — dibuktikan lewat DOM.

| Hal | Isi |
| --- | --- |
| Status | ✅ **SELESAI — 11 Oktober 2026 (malam)**: 8 dari 8 acceptance terbukti (§7) |
| Implementation | Enam berkas §3.2 dan dua berkas delta §10; source tidak berubah sejak build 20.51 WIB |
| Developer verification | `eslint` 0 error; unit 11/11 (uji IGD 115/115); `npm run build` 489/489; putaran Antigravity §9, uji ulang §11, dan R-16 §12 pada bukti mentah dan log backend |
| UAT | Belum dijalankan — diserahkan ke tim UAT; bukan klaim UAT |
| Wewenang UI yang dipakai | `DEV_DISCRETION` §16.5 (judul kelompok, letak isian alasan); delta `IGD-DEC-256` (penanda pada DPJP aktif) |
| Dependency backend | `BE-IGD-056` ✅ — rilis wajib bersama |
| Risiko tersisa | Rilis tanpa `BE-IGD-056` membuat pemilih tidak menerima kelompok kedua dan alasan override tidak tersimpan; lingkungan tanpa jadwal IGD menampilkan keterangan roster kosong dan setiap penetapan menjadi override (`IGD-UNK-18`) |
| Status Git | Enam berkas §3.2 dan dua berkas delta §10 (`M`), ditambah berkas pendaftaran milik pemilik (`M`) |
| Langkah berikutnya | Pemilik: commit; rilis bersama `BE-IGD-056` (R3.17.3) |
