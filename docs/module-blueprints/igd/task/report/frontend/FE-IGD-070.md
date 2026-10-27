# Laporan Perubahan Frontend — `FE-IGD-070`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-070` |
| Judul | Tombol Pasien Tanpa Identitas di langkah Pasien Baru Pendaftaran IGD dan pencabutan blok penanda di Assesmen Awal |
| Slice | `EPIC IGD-17` · Vertical Slice F6 / Pendaftaran Pasien Tanpa Identitas |
| Roadmap | `docs/module-blueprints/igd/roadmap/frontend-roadmap.md` bagian **R3.17** |
| Trace | `IGD-DEC-347` (letak pemicu, menggantikan `IGD-DEC-338`), `IGD-DEC-348` (formulir dua isian), `IGD-DEC-339`…`346`; fakta `IGD-FACT-200`…`213`; menegakkan `IGD-DEC-151` dan `IGD-DEC-161`; requirement `FR-IGD-127` (`AT-IGD-248`) |
| Contract version | **Nol perubahan kontrak.** Endpoint yang dipakai sudah ada dan tidak disentuh task ini |
| Wewenang UI | Letak tombol, pola nama sementara, prasetel penjamin, dan pembatasan formulir menjadi dua isian **sudah diputuskan pemilik** (`IGD-DEC-347`, `339`, `344`, `348`) — bukan `DEV_DISCRETION` |
| Dependency | **NONE** — nol task backend, nol task frontend prasyarat |
| Klasifikasi | `MEDIUM` — tujuh berkas, nol endpoint baru, nol state baru; bobotnya datang dari berkas yang dipakai bersama lintas modul, bukan dari rumitnya logika |
| Task mode | `FRONTEND` — frontend target tulis, backend read-only kecuali berkas laporan dan tautan buktinya |
| Target tulis | `QuilvianSystemFrontendDev` (source), ditambah `docs/module-blueprints/igd/**` pada `NewQuilvianSystemBackend` sebatas laporan, roadmap, dan traceability |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a244a729f` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `8577462f` (branch `rizkiG`) |
| Tanggal | 21 Oktober 2026 |
| Status | ✅ **SELESAI — 27 Oktober 2026: 14 dari 14 acceptance terbukti lewat uji layar (bagian 9); `npm run build` lulus, BUILD_ID `HXGe2JMsrS4xsJSmiOFKW`.** *Sebelumnya pada hari yang sama:* 🟡 14/14 di layar, menunggu build. *Sebelumnya (21 Oktober 2026):* 🟡 **Sebagian** — seluruh source selesai dan ESLint bersih, tetapi 13 dari 14 acceptance criteria menuntut bukti uji layar yang menjadi tugas pemilik. Belum ada satu pun tangkapan layar |

---

## 1. Keadaan yang ditemukan di awal

Pemilik menunjukkan dua tangkapan layar **Pendaftaran Pasien IGD → Pasien Baru**: seluruh tanda
wajib menyala merah — *"Tempat lahir wajib diisi"*, *"Nomor identitas wajib diisi"*, *"Alamat wajib
diisi"* — pada pasien yang justru tiba tanpa membawa apa pun.

Yang ditemukan dari source pada SHA di atas:

| Temuan | Bukti |
| --- | --- |
| Dua belas tanda wajib itu **murni aturan frontend**. Server hanya mewajibkan nama | `REQUIRED_NEW_PATIENT_FIELDS`; `CreatePatientRequest` hanya memberi `[Required]` pada `FullName` (`PatientDtos.cs:341-343`) |
| Pemeriksaan NIK ganda dilewati bila NIK kosong, sehingga banyak rekam pengganti tidak saling menolak | `PatientController.ValidateRequestAsync` (`:2304-2324`) |
| Rekam pasien pengganti **tetap wajib ada** karena `RegPatientEncounter.PatientId` non-nullable | `RegPatientEncounter.cs:26` |
| Tanggal lahir kosong berubah menjadi **waktu sekarang**, sehingga pasien dewasa tercatat lahir hari ini dan umurnya terbaca `0 tahun` | `buildPatientPayload` memakai `toIsoDateOrNow` |
| Jalur pasien tanpa identitas **sudah ada** di triage dan Assesmen Awal, tetapi **tidak ada** pintunya di pendaftaran | `emergency-triage-start-dialog.jsx:345-370`; `emergency-assessment-initial-tab.jsx:735-758` sebelum pencabutan |
| Kategori penjamin **sudah** berawal Tunai untuk seluruh pendaftaran IGD | `DEFAULT_ENCOUNTER_FORM_VALUES.paymentCategory = PAYMENT_CATEGORY.CASH` |
| Baris aksi panel scan sudah memuat tiga tombol sejenis, sehingga tombol keempat sejajar maknanya | `scannerActionRow` pada `patient-selection-step.jsx` (`IGD-FACT-211`) |

Celahnya karena itu bukan "fitur belum ada", melainkan **pintu masuk yang hilang** bagi alur yang
sudah disetujui `IGD-DEC-151` pada 22 September 2026.

### 1.1 Dua koreksi pemilik selama pengerjaan

| Koreksi | Isi | Akibatnya pada pekerjaan |
| --- | --- | --- |
| `IGD-DEC-347` (tinjauan tim analis) | Pemicunya **bukan** kartu ketiga pada layar pilihan jenis pasien, melainkan tombol di dalam langkah *Data Pasien Baru*, karena jalur ini pada hakikatnya **adalah** pendaftaran pasien baru — bukan alur tersendiri | `patient-entry-choice-step.jsx` beserta CSS-nya **dikembalikan utuh ke baseline**; jumlah berkas turun dari sembilan menjadi tujuh, dan Admisi Rawat Inap keluar dari radius dampak |
| `IGD-DEC-348` | Formulir pada jalur ini hanya menampilkan **Nama Lengkap** dan **Jenis Kelamin**, bukan sekadar melepas tanda wajibnya | `NewPatientForm` mendapat prop `minimalIdentityOnly`; `identityType` ikut dikosongkan supaya pasien tanpa kartu identitas tidak tercatat berjenis identitas `"KTP"` |

---

## 2. Proses bisnis dari sisi pengguna

Penggunanya **petugas loket IGD**. Layar dibuka ketika pasien gawat darurat tiba tanpa kartu
identitas dan belum dapat menyebutkan datanya — misalnya korban kecelakaan yang tidak sadar.

1. Petugas membuka **Pendaftaran Pasien IGD** dan memilih **Pasien Baru** seperti biasa. Layar
   pilihan jenis pasien **tidak berubah**: tetap dua kartu.
2. Pada langkah *Data Pasien Baru*, panel **Scan Kartu Identitas** kini memuat **tiga** tombol:
   *Input Manual*, **Pasien Tanpa Identitas**, dan *Cek Scanner* (ditambah *Scan KTP* bila scanner
   siap).
3. Petugas menekan **Pasien Tanpa Identitas**. Formulir pasien baru **yang sama** terbuka, tetapi
   hanya menampilkan **dua isian**:
   - **Nama Lengkap**, sudah terisi otomatis, contohnya `Mr./Mrs. X IGD 2110-1432` — dibaca "pasien
     X, IGD, 21 Oktober, pukul 14.32". Isinya dapat diubah petugas kapan saja.
   - **Jenis Kelamin**, satu-satunya isian yang masih harus dipilih.

   Judul kecil berubah menjadi **DATA PASIEN TANPA IDENTITAS**, lencana **Tanpa Identitas** muncul,
   dan satu kotak keterangan biru menjelaskan bahwa perawat triase yang akan menandai kunjungan ini
   saat Mulai Triage. Bagian *Dokumen dan Kontak*, *Alamat Pasien*, dan *Data tambahan pasien* tidak
   ditampilkan sama sekali.
4. Begitu **Jenis Kelamin** dipilih, sapaan pada nama sementara ikut berubah: *Laki-laki* menjadi
   `Tn. X IGD 2110-1432`, *Perempuan* menjadi `Ny. X IGD 2110-1432`. Tanggal dan jamnya **tidak**
   berubah. Bila petugas sudah mengetik nama sendiri, nama itu **tidak** ditimpa.
5. Petugas menekan lanjut. Rekam pasien tersimpan dengan tanggal lahir **kosong** (bukan hari ini),
   NIK kosong, jenis identitas kosong, dan No. RM terbit seperti pasien lain.
6. Langkah penjamin sudah berada pada kategori **Tunai/Umum** sehingga tidak perlu mencari asuransi
   milik pasien yang identitasnya belum diketahui. Petugas memilih metode pembayaran sistem lalu
   menyelesaikan pendaftaran.
7. Pasien tampil pada daftar **Menunggu Triage** dengan nama sementaranya. **Perawat triase**
   menekan *Mulai Triage*, mencentang *Pasien tanpa identitas*, dan mengisi *Nama sementara*. Sejak
   titik itu kunjungan bertanda `IsUnknownPatient` dan seluruh dokumentasi klinis berjalan normal.

Jalur tidak normal:

| Keadaan | Yang terjadi |
| --- | --- |
| Petugas salah tekan, ternyata pasien membawa identitas | Tekan **Input Manual**. Formulir kembali menampilkan seluruh bagiannya, nama sementara yang belum disunting dikosongkan, dan jenis identitas kembali ke `"KTP"` |
| Petugas mengosongkan **Jenis Kelamin** | Tetap ditolak dengan pesan *"Jenis kelamin wajib dipilih."* — field ini sengaja tetap wajib karena dipakai untuk dosis, penempatan tempat tidur, dan skrining kehamilan |
| Petugas mengosongkan **Nama Lengkap** | Tetap ditolak. Nama adalah satu-satunya isian yang diwajibkan server, dan isinya sudah terisi otomatis sehingga penjaga ini praktis tidak pernah terasa |
| Dua pasien tanpa identitas tiba pada menit yang sama | Keduanya tetap tersimpan. Nama sementaranya sama, tetapi No. RM-nya berbeda, dan petugas dapat mengubah salah satunya |
| Petugas menekan *Kembali ke Pilih Jenis Pasien* | Formulir dikosongkan kembali ke nilai awal; penanda tanpa identitas ikut hilang |
| Identitas diketahui kemudian | Di luar lingkup layar ini: pendaftaran ulang lewat alur pasien standar dan penggabungan rekam pasien milik Master Patient (`IGD-DEC-151`) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`; `rules/frontend/base-component-decision-gate.md`; `rules/frontend/ui-consistency-checklist.md`; `rules/frontend/REPORT_TEMPLATE.md`
- `docs/module-blueprints/igd/00-interview-decisions.md` (bagian 21 Oktober 2026 malam, `IGD-DEC-151`, `IGD-DEC-161`, `IGD-DEC-145`)
- `docs/module-blueprints/igd/roadmap/frontend-roadmap.md` bagian R3.17
- Backend read-only: `PatientController.cs`, `PatientDtos.cs`, `MstPatient.cs`, `RegPatientEncounter.cs`, `EmergencyVisitService.cs`, `EmergencyVisitController.cs`, `EmgSetting.cs`
- Frontend: seluruh berkas pada 3.2, ditambah `patient-entry-choice-step.jsx`, `plustek-scan-panel.jsx`, `emergency-triage-start-dialog.jsx`, `emergency-management-triage-utils.jsx`, `use-outpatient-registration.js`, `inpatient-admission-patient-utils.jsx`, `inpatient-admission-view.jsx`, `outpatient-registration-page.jsx` sebagai pemeriksaan radius dampak

### 3.2 Berkas yang berubah

Tujuh berkas, seluruhnya di repository frontend.

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../emergency-registration.constants.js` | `UNKNOWN_IDENTITY_FLAG_FIELD`, `UNKNOWN_IDENTITY_ALIAS_FIELD`, `UNKNOWN_IDENTITY_ALIAS_MARKER`, `UNKNOWN_IDENTITY_SALUTATIONS`, `UNKNOWN_IDENTITY_OPTIONAL_FIELDS`, dan dua isian tersembunyi pada `DEFAULT_EMERGENCY_REGISTRATION_FORM_VALUES`. `REQUIRED_NEW_PATIENT_FIELDS`, `PATIENT_ENTRY_MODE`, dan `PATIENT_ENTRY_OPTIONS` **tidak disentuh** |
| `src/utils/.../emergency-registration.utils.js` | `buildUnknownIdentityAlias` dan `replaceUnknownIdentitySalutation`; `buildPatientPayload` mengirim `birthDate` `null` **hanya** ketika penanda tanpa identitas menyala, selebihnya tetap `toIsoDateOrNow` seperti sebelumnya |
| `src/lib/hooks/.../use-emergency-registration.js` | Dua aksi baru `handleUseUnknownIdentityPatient` dan `handleUseIdentifiedPatient`; penyelarasan sapaan saat Jenis Kelamin berubah; `handlePatientStepNext` menyaring field wajib secara lokal; `handlePatientModeChange` membersihkan penanda saat jenis pasien dipilih ulang |
| `src/components/view/.../emergency-registration-page.jsx` | Meneruskan kedua aksi itu ke `PatientSelectionStep` |
| `src/components/view/.../patient-selection-step.jsx` | Tombol *Pasien Tanpa Identitas* pada baris aksi panel scan (tampil hanya bila prop diberikan), *Input Manual* memanggil aksi pengembalian, judul/lencana/kotak keterangan bersyarat, serta penerusan `optionalFieldNames` dan `minimalIdentityOnly` |
| `src/components/view/.../new-patient-form.jsx` | Prop `optionalFieldNames` (default kosong) dan `minimalIdentityOnly` (default mati). Saat `minimalIdentityOnly` menyala, komponen menampilkan satu bagian berisi Nama Lengkap dan Jenis Kelamin saja; pohon formulir lengkapnya tidak disentuh |
| `src/components/view/.../emergency-assessment-initial-tab.jsx` | **Pencabutan** blok centang *Pasien tanpa identitas (Mr./Mrs. X)*, field *Nama sementara*, konstanta `TEMPORARY_ALIAS_FIELD`, dan penjaga *"Nama sementara wajib diisi…"*. Nilai `isUnknownPatient` dan `temporaryPatientAlias` **tetap** disimpan di state, tetap dimuat dari server, dan **tetap dikirim** pada payload `PUT` |

Dua berkas yang sempat diubah pada percobaan pertama — `patient-entry-choice-step.jsx` dan
`patient-entry-choice-step.module.css` — **dikembalikan utuh ke baseline** sesudah `IGD-DEC-347`.
`git diff` keduanya kosong, dan `npx eslint` atas keduanya bersih.

### 3.3 Kepatuhan arsitektur frontend

- **Opt-in untuk seluruh berkas yang dipakai bersama.** Tombol baru dijaga prop
  `onUseUnknownIdentityPatient`, pembatasan formulir lewat prop `minimalIdentityOnly` yang berawal
  mati, pelepasan tanda wajib lewat `optionalFieldNames` yang berawal kosong, dan cabang `birthDate`
  hanya aktif pada jalur tanpa identitas. Pendaftaran Rawat Jalan dan Admisi Rawat Inap karena itu
  menerima **nol** perubahan perilaku.
- **Nol arsitektur baru.** Tanpa slice Redux baru, tanpa service baru, tanpa komponen baru. Penanda
  jalur disimpan sebagai isian form berawalan `__`, meniru `__scanResolvedRegion` yang sudah dipakai
  berkas yang sama.
- **Komponen dipakai ulang.** `EmergencyInlineAlert`, `EmergencySelectField`, dan
  `EmergencyTextField` dipakai apa adanya.
- **`globals.css` tidak disentuh**, dan sesudah `IGD-DEC-347` **nol berkas CSS** disentuh task ini.
- **Nol baris komentar baru** pada seluruh berkas; komentar lama tidak disunting.

#### Tabel keputusan base component (`UI GATE`)

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Tombol pemicu pada baris aksi panel scan | Tombol `styles.secondaryButton` yang sudah dipakai *Input Manual*, *Cek Scanner*, *Scan KTP* | `scannerActionRow` pada `patient-selection-step.jsx` | `REUSE` | Tombol keempat memakai class dan markup yang sama persis dengan tiga tetangganya |
| Kotak keterangan jalur tanpa identitas | `EmergencyInlineAlert` | `emergency-registration-fields.jsx`, sudah dipakai berkas yang sama | `REUSE` | `tone="info"` dengan judul dan isi |
| Lencana penanda jalur | `styles.manualEntryBadge` | Sudah dipakai untuk lencana *Input Manual* | `REUSE` | Teks lencana diganti bersyarat |
| Formulir dua isian | `EmergencyTextField`, `EmergencySelectField` | Keduanya sudah dipakai formulir yang sama | `REUSE` | Dirangkai dalam satu `formSection` dan `formGrid2` yang sudah ada |
| Pelepasan tanda wajib pada field form | Prop `rules` pada ketiga field wrapper | Ketiganya sudah menerima `rules` | `REUSE` | `rules` dikirim `undefined` untuk field yang dilepas |

**Seluruh baris berstatus `REUSE`; nol `EXTEND`, nol `COMPOSE`, nol `WRAP`, nol `NEW`.** Pada
percobaan pertama ada satu baris `EXTEND` — tema kartu keenam beserta ilustrasinya pada komponen
pilihan jenis pasien — dan baris itu **hilang seluruhnya** sesudah `IGD-DEC-347` memindahkan
pemicunya; komponen tersebut kini tidak disentuh.

**Penyimpangan yang dicatat:** tombol baru memakai `<button className={styles.secondaryButton}>`,
bukan `BaseButton` seperti anjuran `ui-consistency-checklist.md` bagian C. Alasannya tombol ini
berdiri di satu baris bersama tiga tombol sejenis yang seluruhnya memakai markup itu; memakai
`BaseButton` justru membuat satu tombol tampil berbeda sendiri di antara tetangganya. `AGENTS.md`
("ikuti kode yang sudah ada") didahulukan, dan selisihnya dilaporkan di sini.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah dari sebelumnya. Jalur ini tidak menambah permintaan server baru, dan pada mode dua isian tidak ada lagi pilihan wilayah yang perlu dimuat sama sekali |
| Kosong | Formulir dua isian tidak punya keadaan kosong: Nama Lengkap selalu terisi otomatis |
| Gagal | Kegagalan penyimpanan pasien tetap tampil seperti sebelumnya lewat pesan langkah pendaftaran, misalnya *"Gagal menyimpan pasien baru."* |
| Tanpa hak akses | `NOT APPLICABLE` — `IGD-DEC-345` menetapkan tanpa permission baru, sehingga tidak ada keadaan tanpa hak akses yang khusus bagi jalur ini |

---

## 5. Endpoint yang dikonsumsi

Task ini **tidak menambah maupun mengubah** pemanggilan API.

#### Patient

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/patient-management/master-data/patients` | Melahirkan rekam pasien pengganti. Hanya `fullName` yang diwajibkan server; pada jalur ini `birthDate` dikirim `null` dan `identityType` dikirim kosong | `Patient : Create` |

#### PatientEncounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/patient-encounters` | Melahirkan encounter `Emergency` bagi rekam pasien pengganti | `PatientEncounter : Create` |

#### EmergencyVisit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/v1/health-services/emergency-installation-management/emergency-visits/{id}` | Pembaruan data kedatangan dari Assesmen Awal. Payload **tetap** membawa `isUnknownPatient` dan `temporaryPatientAlias` walau kendali layarnya dicabut, supaya penanda yang diisi di Mulai Triage tidak terhapus | `EmergencyVisit : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas tujuh berkas yang berubah | 0 error, 7 warning | `PASS` | Keluaran perintah |
| Pembandingan baseline: `eslint` atas salinan versi `HEAD` dari dua berkas yang menghasilkan warning | 0 error, **7 warning yang sama persis** (`react-hooks/set-state-in-effect` 3, `react-hooks/exhaustive-deps` 2, `react-hooks/preserve-manual-memoization` 2) | `EXISTING / ENVIRONMENT ISSUE` | Salinan sementara dibuat dari `git show HEAD:<path>`, di-lint, lalu dihapus; `git status` bersih dari berkas sementara |
| `npx eslint` atas dua berkas yang dikembalikan ke baseline | Bersih, dan `git diff` keduanya kosong | `PASS` | Keluaran perintah |
| Uji layar seluruh acceptance criteria | Belum dijalankan | `NOT RUN` | — |

**Uji manual: `NOT FEASIBLE`.** Alasan konkretnya: port 3000 sedang dipakai `next dev` milik pemilik
(PID 22008), uji layar modul ini memang dikerjakan pemilik, dan menjalankan build produksi atau dev
server tandingan membutuhkan izin terpisah. Ringkasan agen juga tidak diterima sebagai bukti uji
pada modul ini.

**`AUTOMATED TEST: SKIPPED (opsional)`** — repository ini tidak mewajibkan test otomatis untuk task
frontend, dan pemilik melarang agent menulis maupun menjalankan unit test.

**Tidak dijalankan:** `npm run build` (port dan proses build milik pemilik), `npm test` (dilarang),
dan seluruh uji layar.

### 6.1 Grep anti-regresi konsistensi UI

Dijalankan hanya atas **baris yang ditambahkan** pada tujuh berkas yang diubah.

| No | Pemeriksaan | Hasil | Keterangan |
| ---: | --- | --- | --- |
| 1 | Warna literal pada stylesheet | **0** | Sesudah `IGD-DEC-347`, task ini tidak menyentuh berkas CSS mana pun |
| 2 | `font-size` / `font-weight` / `line-height` menimpa komponen shared | **0** | — |
| 3 | `<button>` mentah atau class `btn` Bootstrap baru | **1 temuan — dipertahankan** | Tombol *Pasien Tanpa Identitas* memakai markup dan class yang sama dengan tiga tombol tetangganya di baris aksi yang sama; alasannya pada 3.3 |
| 4 | `<table>` mentah | **0** | — |
| 5 | Utility typography Bootstrap di dalam blok tabel | **0** | — |
| 6 | `!important` baru | **0** | — |
| 7 | `globals.css` tersentuh | **0 berkas** | — |
| 8 | Baris komentar baru pada tujuh berkas | **0** | — |
| 9 | Inline style untuk nilai statis | **0** | — |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1 — Baris aksi panel scan memuat tombol *Pasien Tanpa Identitas*; layar pilihan jenis pasien tetap dua kartu | Belum terpenuhi — kode siap, bukti layar belum | `scannerActionRow` pada `patient-selection-step.jsx`; `patient-entry-choice-step.jsx` `git diff` kosong |
| 2 — Rawat Jalan tidak menampilkan tombol itu; Admisi Rawat Inap tidak berubah | Belum terpenuhi — kode siap, bukti layar belum | Tombol dijaga prop `onUseUnknownIdentityPatient` yang hanya dikirim halaman IGD |
| 3 — Tombol membuka formulir pasien baru **yang sama** dengan Nama Lengkap terisi berpola; *Input Manual* mengembalikannya | Belum terpenuhi — kode siap, bukti layar belum | `handleUseUnknownIdentityPatient` dan `handleUseIdentifiedPatient` |
| 4 — Sapaan mengikuti Jenis Kelamin | Belum terpenuhi — kode siap, bukti layar belum | `replaceUnknownIdentitySalutation` + penyelarasan di hook |
| 5 — Formulir hanya menampilkan dua isian | Belum terpenuhi — kode siap, bukti layar belum | Prop `minimalIdentityOnly` pada `new-patient-form.jsx` |
| 5a — Formulir pasien baru biasa tetap lengkap | Belum terpenuhi — kode siap, bukti layar belum | Prop berawal mati; pohon formulir lengkap tidak disentuh |
| 6 — Jenis Kelamin kosong tetap ditolak | Belum terpenuhi — kode siap, bukti layar belum | Aturan `validate` pada field tetap tanpa syarat |
| 7 — `birthDate` `null` dan `identityType` kosong; umur tidak tampil `0 tahun` | Belum terpenuhi — kode siap, bukti layar belum | Cabang `buildPatientPayload`; pengosongan `identityType` di hook |
| 8 — Dua pendaftaran berurutan tidak bentrok NIK | Belum terpenuhi — kode siap, bukti layar belum | `IGD-FACT-201` |
| 9 — Langkah penjamin terbuka pada Tunai dan dapat dilanjutkan | Belum terpenuhi — bukti layar belum | **Sudah terpenuhi sejak awal oleh nilai awal `DEFAULT_ENCOUNTER_FORM_VALUES`; nol baris kode ditambahkan** |
| 10 — Pasien tampil di Menunggu Triage; Mulai Triage tetap memuat centang dan nama sementara | Belum terpenuhi — bukti layar belum | `emergency-triage-start-dialog.jsx` tidak disentuh |
| 11 — Assesmen Awal tidak lagi memuat blok penanda; seksi 1 selebihnya utuh | Belum terpenuhi — kode siap, bukti layar belum | Diff `emergency-assessment-initial-tab.jsx` (−48 baris, 0 penambahan) |
| 12 — Pasien tanpa identitas dapat ditriase dan diperiksa sampai pengkajian tersimpan | Belum terpenuhi — bukti layar belum | — |
| 13 — Base component bersama tidak disentuh, `globals.css` tidak disentuh, ESLint 0 error 0 warning, nol baris komentar baru | **Terpenuhi sebagian — tiga dari empat bagian.** Base component tidak disentuh ✔, `globals.css` tidak disentuh ✔, nol komentar baru ✔, ESLint 0 error ✔ — tetapi **bukan** 0 warning: 7 warning bertahan, seluruhnya sudah ada sebelum task ini | Bagian 6 dan 6.1 |

**Definition of Done:** **nol dari 14 kriteria terbukti penuh.** Kriteria 13 terpenuhi pada tiga dari
empat bagiannya; tiga belas kriteria lain menunggu uji layar pemilik. Laporan tracked ada. Roadmap
dan traceability diperbarui. Status task **🟡 sebagian**.

Catatan atas bagian "0 warning" pada kriteria 13: tujuh warning itu berasal dari dua berkas yang
memang disentuh task ini, tetapi seluruhnya **sudah ada sebelum task ini** dan terbukti identik pada
salinan versi `HEAD` (bagian 6). Membereskannya berarti menyentuh `useCallback`, daftar dependency,
dan `useEffect` milik alur pendaftaran yang tidak diminta kartu ini, sehingga sengaja **tidak**
dikerjakan dan dilaporkan apa adanya alih-alih diklaim lulus.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | ESLint memunculkan 7 warning pada dua berkas, seluruhnya **sudah ada sebelum task ini** dan terbukti identik pada salinan versi `HEAD` |
| Masalah yang diketahui | **Koreksi atas kartu task:** kartu semula menyebut *sebelas* tanda wajib dilepas termasuk Nama Lengkap. Yang dikerjakan **sepuluh** — Nama Lengkap tetap wajib karena ia satu-satunya isian yang diwajibkan server (`IGD-FACT-200`); melepasnya hanya memindahkan penolakan dari pesan di layar menjadi galat `400`, sementara isinya toh sudah terisi otomatis |
| | **Kolom Catatan Pasien ikut tersembunyi** akibat `IGD-DEC-348`. Pada praktiknya kolom itu kerap dipakai mencatat ciri pasien tanpa identitas — misalnya *"laki-laki perkiraan 45 tahun, kaos merah, korban kecelakaan"* — yang justru menjadi bahan mengenali pasien kemudian. Dijalankan apa adanya sesuai instruksi pemilik; bila ciri pasien perlu tercatat, kolom itu dapat dikembalikan lewat keputusan susulan |
| | Nama sementara dua pasien yang tiba pada menit yang sama akan sama persis. Diterima: No. RM-nya tetap berbeda dan petugas dapat mengubahnya |
| | Sesudah blok di Assesmen Awal dicabut, penanda tanpa identitas tidak lagi dapat dikoreksi dari ruang kerja perawat (`IGD-DEC-341`, konsekuensi yang sudah diterima pemilik) |
| Dependency backend | **NONE** |
| Perubahan sampingan | **NONE.** Dua berkas percobaan pertama dikembalikan utuh ke baseline, dan hanya dua berkas itu. Working tree juga memuat perubahan sesi lain yang **tidak** disentuh task ini: `emergency-locked-notes-tab.jsx`, `emergency-doctor-progress-tab.jsx`, `emergency-assessment-disposition-tab.jsx`, `emergency-assessment-section.jsx`, `soap-editor.jsx`, `use-emergency-progress-note.jsx`, `emergency-assessment.module.css`, dan berkas baru `emergency-disposition-empty-state.jsx` |
| Interupsi | Dua koreksi pemilik di tengah pengerjaan (`IGD-DEC-347` dan `IGD-DEC-348`). Pemulihannya: dua berkas dikembalikan ke baseline dan diverifikasi lewat `git diff` kosong serta `eslint` bersih, bukan ditinggalkan sebagai kode mati |
| Status Git | Tujuh berkas task ini berstatus `M` pada branch `RizkiV2`; nol berkas di-stage, nol commit, nol push |
| Langkah berikutnya | Pemilik menjalankan uji layar 13 kriteria yang tersisa, terutama kriteria 2 (Rawat Jalan tidak menampilkan tombol dan Admisi Rawat Inap tidak berubah), kriteria 5a (formulir pasien baru biasa tetap lengkap), dan kriteria 11 (seksi 1 Assesmen Awal tetap utuh). Sesudah buktinya ada, status task dinaikkan menjadi ✅ |

---

## 9. Pengerjaan lanjutan — 27 Oktober 2026, uji layar dan dua perbaikan

Uji layar dijalankan agen pada `npm run dev` pemilik (port 3000) dan backend `https://localhost:7184`,
Playwright `msedge` headless. Bukti di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-fe-igd-070-20261027/`
(di-gitignore). Setiap verdict dihitung dari nilai DOM atau jaringan yang diukur; `hasil.json` dirakit
`skrip/rakit-hasil.mjs` yang memetakan setiap acceptance ke pemeriksaan sah. PNG diperiksa langsung.

### 9.1 Dua penghalang yang ditemukan uji, beserta perbaikannya

| # | Penghalang | Bukti | Keputusan | Perbaikan |
| ---: | --- | --- | --- | --- |
| 1 | **Petugas loket ditolak `403` saat menyimpan pasien baru.** Service pendaftaran memanggil rute **kiosk** — `POST patients`, `GET patient-insurances`, `GET/POST patient-company-guarantors` — yang dijaga `KioskReadPolicy` (hanya SuperAdmin, Administrator, Kiosk; `Program.cs:1259`). Izin loket sendiri **lengkap**: `POST patients/admin` dan `patient-encounters/admin` menjawab `400` validasi, bukan `403` (`02-sonde-rute-admin.json`). Tidak ada izin role-access yang diubah | `IGD-FACT-249` | `IGD-DEC-355` — rute staf **khusus IGD, opt-in**; Rawat Jalan tidak berubah | `emergency-registration.service.js`: tiga kunci URL `…ForStaff` dan opsi `staffRoute` berawal `false` pada lima fungsi; `emergency-registration-slice.jsx`: keempat thunk IGD menyalakan `staffRoute: true` |
| 2 | **Tidak ada layar yang dapat menandai kunjungan sebagai pasien tanpa identitas.** Dialog Mulai Triage dilepas `8cfbfa971` (15 Oktober 2026), enam hari sebelum task ini mencabut blok di Assesmen Awal (`IGD-DEC-341`) dengan anggapan penanda tetap diisi di Mulai Triage (`IGD-DEC-340`). Kunjungan B1 lahir `isUnknownPatient: false` | `IGD-FACT-250` | `IGD-DEC-356` — dialog dipasang ulang | `emergency-triage-patient-list-view.jsx`: import, state target/galat, dua handler, dan render `EmergencyTriageStartDialog` dikembalikan persis seperti sebelum `8cfbfa971`. Komponen dialog tidak disentuh |

Ketiga berkas: ESLint **0 error / 0 warning** sebelum dan sesudah (identik baseline), CRLF dipertahankan,
nol baris komentar baru, `globals.css` dan komponen bersama tidak disentuh. Pemakai lima fungsi service
selain IGD hanya `outpatient-registration-slice.jsx`, yang tetap memanggil tanpa opsi — perilakunya
tidak berubah. Admisi Rawat Inap sudah memakai `/admin` (`inpatient-admission-payment.service.js:35`–`:36`).

### 9.2 Skenario

| Berkas | Skenario | Akun | Verdict berkas | Catatan |
| --- | --- | --- | :---: | --- |
| `hasil-A.json` | Formulir: dua kartu, baris aksi, alias, sapaan, dua isian, Input Manual, penolakan | loket | **PASS 15/15** | Tanpa data tercipta |
| `hasil-B1.json`, `hasil-B2.json` | Dua pendaftaran tanpa identitas berurutan sampai layar Selesai | loket | FAIL 10/12 | Dua pemeriksaan **salah rancang**: menuntut `201` sesuai teks kartu, `CreatePatientForAdmin` menjawab `200`. Verdict tidak diubah |
| `hasil-B-ukur-ulang.json` | Ukur ulang B dari jaringan yang sama + `GET` detail pasien | loket, perawat | FAIL 16/18 | Dua pemeriksaan *kunjungan terbentuk saat pendaftaran* **salah rancang**: IGD encounter-first, kunjungan lahir saat Mulai Triage — diukur di C |
| `hasil-C.json` | Mulai Triage dengan dialog pada pasien B2 sampai pengkajian | perawat | **TIDAK SAH** | `checks` kosong akibat bug skrip (`activeChecks` menunjuk larik yang sama lalu dikosongkan); verdict `PASS` tertulis **tidak dipakai** |
| `hasil-C-ukur-ulang.json` | Ukur ulang C dari `jaringan-C.json` + `GET` detail kunjungan | perawat | **PASS 6/6** | Label dialog dinilai dari `C2-dialog-mulai-triage.png` |
| `hasil-D.json` | Triase dan pengkajian pasien B1 (kunjungan tanpa penanda) | perawat | FAIL 8/9 | Satu pemeriksaan **salah rancang**: `input[name=arrivalLocation]` tidak merender atribut `name` |
| `hasil-D-ukur-ulang.json` | Seksi 1 Assesmen Awal dari teks DOM terekam | — | **PASS 2/2** | |
| `hasil-E.json`, `hasil-E-ukur-ulang.json` | Rawat Jalan dan Admisi | loket, perawat | FAIL | Admisi wizard 12 langkah; formulir ada di langkah 3 — pemeriksaan salah rancang dua kali |
| `hasil-E-ukur-ulang-2.json` | Admisi sampai langkah Pendaftaran, nol panggilan tulis | perawat | **PASS 3/3** | |

Akun perawat `dimas.kurniawan` berperan SuperAdmin (`IGD-FACT-241`) — satu-satunya akun perawat yang
tersedia; acceptance task ini tidak menyangkut otorisasi. Langkah loket memakai `rendy.saputra`
(tanpa peran SuperAdmin), sesuai alur bisnis.

### 9.3 Acceptance

| # | Keadaan | Bukti terukur |
| ---: | :---: | --- |
| 1 | ✅ | Dua kartu `btn-select-patient-type-existing/new`; baris aksi *Input Manual*, *Pasien Tanpa Identitas*, *Cek Scanner* — `A1-*.png` |
| 2 | ✅ | Rawat Jalan: baris aksi hanya *Input Manual* dan scanner, nol tombol tanpa identitas (`E1-*.png`); Admisi langkah 3 formulir pasien baru tampil tanpa tombol, nol panggilan tulis (`E5-admisi-formulir-pasien.png`); commit task `52624e6cb` menyentuh nol berkas Rawat Inap |
| 3 | ✅ | Alias `Mr./Mrs. X IGD 2710-0932`, selisih 0 menit dari klik; dapat disunting; *Input Manual* mengosongkan alias yang belum disunting |
| 4 | ✅ | `Tn.` / `Ny.` / `Mr./Mrs.` terukur — `A4-sapaan-*.png`. Lihat catatan 9.4 butir 1 |
| 5 | ✅ | Formulir hanya `fullName` dan `gender`; *Dokumen dan Kontak*, *Alamat Pasien*, *Data tambahan* tidak tampil; `POST patients/admin` **200** *"Patient berhasil dibuat."* (kartu menyebut 201) dua kali, nol pesan *wajib diisi* |
| 5a | ✅ | Sesudah *Input Manual*: tiga bagian dan isian lengkap kembali — `A5a-formulir-input-manual.png` |
| 6 | ✅ | Jenis Kelamin bawaan (*Tidak diketahui*) ditolak *"Jenis kelamin wajib dipilih."*, nol `POST` — `A6-*.png` |
| 7 | ✅ | Muatan `birthDate: null`, `identityType: ""`; rekam tersimpan `birthDate` `null`; nol *"0 tahun"* di daftar triase, daftar pemeriksaan, dan header pasien (`D4-pengkajian-tersimpan.png`: *UMUR / GENDER Laki-laki*) |
| 8 | ✅ | Dua pendaftaran berurutan, No. RM `00-00-02-82` dan `00-00-02-83`, nol `PATIENT_IDENTITY_ALREADY_EXISTS` |
| 9 | ✅ | Kartu *Tunai* aktif, ringkasan *Pembayaran Tunai / Umum — Dipilih*, tabel penjamin tidak tampil, lanjut ke verifikasi tanpa penjamin. Lihat catatan 9.4 butir 2 |
| 10 | ✅ | Pasien tampil di *Menunggu Triase* dengan nama alias (`C1-baris-pasien.png`); dialog Mulai Triage memuat *Pasien tanpa identitas* dan *Nama sementara* (`C2-dialog-mulai-triage.png`); `start-triage` **201** membawa `isUnknownPatient: true` + alias; `GET` detail kunjungan `IGD-261027031823-B9CDD2` menyimpan keduanya |
| 11 | ✅ | Tab Assesmen Awal tanpa centang dan tanpa *Nama sementara*; seksi 1 utuh (sembilan label); *Perbarui Data Kedatangan* `PUT emergency-visits` **200** *"Data kunjungan IGD berhasil diubah."* dan tetap membawa penanda — `D3-perbarui-data-kedatangan.png` |
| 12 | ✅ | Triase **HIJAU – Prioritas 3** tersimpan `200` untuk kedua pasien (`C3-triase-tersimpan.png`); pengkajian `ASM-20261027-00001` dan `ASM-20261027-00002` **201** |
| 13 | ✅ | Menurut `IGD-DEC-357`: base component dan `globals.css` tidak disentuh; nol komentar baru; ESLint 0 error dan **nol warning baru** — task ini menambah 0 warning (`patient-selection-step.jsx` 5w dan `use-emergency-registration.js` 2w sama persis sebelum dan sesudah `52624e6cb`, diukur `eslint --stdin`); 4 warning `emergency-assessment-initial-tab.jsx` lahir sesudahnya dari pekerjaan lain |

### 9.4 Catatan, tidak dikartukan

1. **Sapaan *Mr./Mrs.* tampil tetapi tidak pernah dapat tersimpan** — nilai bawaan Jenis Kelamin adalah
   *Tidak diketahui* (`0`), dan aturan `Number(value) > 0` menolaknya. Pesan *"Jenis kelamin wajib dipilih."*
   juga tetap menempel sesudah jenis kelamin lain dipilih sampai simpan berikutnya (`A4-sapaan-P.png`) —
   `IGD-FACT-252`.
2. **Isian *Metode Pembayaran Sistem* tidak pernah tampil di Pendaftaran IGD** — `page.jsx` tidak
   meneruskan `paymentMethodOptions`, sehingga klausa *"tetap dipilih petugas"* acceptance 9 tidak
   punya objek; keadaan sejak sebelum task ini, untuk jenis pasien apa pun — `IGD-FACT-251`.
3. **Riwayat Assesmen Awal tampil *"Belum ada assesmen awal"* 3 detik sesudah `ASM-20261027-00001`
   tersimpan** (`D4-pengkajian-tersimpan.png`) — diperiksa pada `FE-IGD-022`.
4. **Rute kiosk yang sama juga dipanggil Pendaftaran Rawat Jalan** — tidak diubah (`IGD-DEC-355`);
   diserahkan ke pemilik Rawat Jalan lewat `IGD-FACT-249`.

### 9.5 Data uji yang tercipta di basis data dev (lewat aplikasi)

| Data | Keterangan |
| --- | --- |
| Pasien `00-00-02-81` *Tn. X IGD 2710-0951* | Percobaan ke-2 B1: pasien tersimpan, langkah pembayaran gagal karena skrip — **tanpa encounter** |
| Pasien `00-00-02-82` *Tn. X IGD 2710-0953*, `ENC-RSMMC-00525`, kunjungan `IGD-261027025825-C11067` | B1: kunjungan dimulai **tanpa** dialog (sebelum `IGD-DEC-356`), `isUnknownPatient: false`; triase HIJAU; `ASM-20261027-00001` |
| Pasien `00-00-02-83` *Ny. X IGD 2710-0954*, `ENC-RSMMC-00526`, kunjungan `IGD-261027031823-B9CDD2` | B2: dialog, `isUnknownPatient: true`; triase HIJAU; `ASM-20261027-00002` |

### 9.6 Validasi

| Perintah | Hasil |
| --- | --- |
| `npx eslint` tiga berkas perbaikan | 0 error / 0 warning — identik baseline |
| `npx eslint --stdin` versi `52624e6cb^`, `52624e6cb`, `HEAD` | Task menambah 0 warning |
| Komentar baru pada diff | 0 |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — aturan pemilik: tanpa unit test |
| `npm run build` (27 Oktober 2026, atas izin pemilik; `next dev` pemilik dihentikan) | **PASS** — `✓ Compiled successfully in 78s`, 517/517 halaman, standalone siap, BUILD_ID `HXGe2JMsrS4xsJSmiOFKW`, nol warning di log; mencakup ketiga berkas perbaikan dan perubahan `FE-IGD-022` (`build-npm-run-build.log`) |
| `MANUAL TEST` | `PASS` — skenario 9.2 |
