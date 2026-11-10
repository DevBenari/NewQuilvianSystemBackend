# Rehab Medik — Interview Decisions

| Field | Nilai |
|---|---|
| Blueprint ID | `RHB-BP-001` |
| Revision | `1` |
| Status | `draft` — scope pass dan closure pass selesai (2026-11-05); siap `design-business-module` |
| Mode wawancara | Scope pass, lalu closure pass atas `01-existing-capability-map.md` |
| Product/domain owner | Sukma Giri Pratama |
| Pengesah klinis | Komite medik / dokter Sp.KFR rumah sakit — **belum ditunjuk namanya** |
| Backend SHA | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`) |
| Frontend SHA | `0c06c09305e6ee9c4f21e85393ebc7de10576550` (`sukmagpV2`) |
| Masukan bisnis | `rehab-medik-deep-analysis.md` (16 September 2026) dan `PRD_Rehab_Medik_Quilvian_V2.md` v1.0 (5 November 2026) |
| Tanggal wawancara | 5 November 2026 |

> **Catatan audit.** Scope dikunci **tanpa** capability map (`01-existing-capability-map.md`
> belum ada). Kemungkinan duplikasi dengan modul lain belum diperiksa menyeluruh. Discovery
> singkat sesi ini hanya menemukan bukti yang dicatat sebagai `Fact` di bawah.

---

## 1. Scope dan Outcome

**Batas modul dalam satu kalimat.** Modul Rehab Medik mengelola satu *episode* rehabilitasi
pasien. Episode dimulai dari permintaan/rujukan, lalu melewati asesmen profesi, program terapi,
sesi terapi, evaluasi, dan discharge. Catatannya kemudian difinalkan, dan sesudah itu hanya
boleh dikoreksi lewat addendum.

### Di dalam scope

Blueprint mencatat **seluruh 33 feature** (FEAT-001..033) agar entity dan status tidak perlu
dirombak saat fase berikutnya. **Rilis pertama (MVP) hanya Phase 1** (RHB-DEC-001).

| Fase | Feature | Keterangan rilis |
|---|---|---|
| Phase 1 — rilis 1 | FEAT-001, 002, 003, 007, 008, 009, 010, 012, 013, 015, 016, 017, 018, 024, 025, 026, 027, 028, 029, **032** | FEAT-011 (asesmen ortotik-prostetik) **ditunda** ke fase perangkat (RHB-DEC-004). FEAT-032 ditarik dari Phase 3 oleh RHB-DEC-021. |
| Phase 2 | FEAT-004, 005, 006, 011, 014, 019, 020, 021, 022, 023 | Masuk blueprint, tidak masuk roadmap rilis 1 |
| Phase 3 | FEAT-030, 031, 033 | Masuk blueprint, menunggu UNR-006/007/008 |

### Di luar scope — untuk modul lain

| Kebutuhan | Pemilik | Titik sentuh dengan Rehab Medik |
|---|---|---|
| Master pasien, nomor RM, identitas | PatientManagement (`Pat`) | Rehab hanya membaca/memilih pasien |
| Registrasi kunjungan, encounter RJ/RWI/IGD | RegistrationManagement (`Reg`), InPatientManagement (`Inp`), EmergencyInstallationManagement (`Emg`) | Permintaan dan setiap sesi wajib tertaut ke encounter existing |
| Tarif, kasir, item billing | BillingManagement (`Bil`) | Phase 3: rehab mengirim layanan final |
| SEP, surat kontrol, grouper, klaim | InsuranceManagement (`Ins`, `PLANNED`) / casemix | Phase 3: rekonsiliasi bukti |
| Kredensial, SDM, jadwal pegawai | HR (`Hrd`/`Wfp`) | Rehab membaca identitas pelaksana |
| Komponen ortotik-prostetik, stok, vendor | Pengadaan/inventori | Phase 2: work order perangkat |
| Order rehab dari layar dokter RJ/RWI/IGD | Modul dokter masing-masing | Slice integrasi setelah rilis 1 (RHB-DEC-008) |
| Rehabilitasi NAPZA, rehabilitasi sosial, tele-rehab, portal pasien, AI saran klinis | — | Tidak dikerjakan |

---

## 2. Glossary

| Istilah | Arti yang dapat diuji |
|---|---|
| Permintaan rehab | Catatan bahwa seseorang meminta layanan rehab untuk pasien tertentu. Permintaan **bukan** asesmen dan **bukan** bukti terapi. |
| Episode rehab | Satu rangkaian layanan rehab untuk satu masalah pasien. Episode dapat berisi banyak kunjungan dan banyak sesi. |
| Disposisi | Keputusan atas permintaan: diterima, perlu klarifikasi, dialihkan, atau ditolak. Setiap disposisi wajib punya alasan. |
| Program aktif | Program terapi satu disiplin yang sudah disusun profesinya **dan** berada di bawah rencana medis Sp.KFR yang final. Sesi hanya boleh merujuk program aktif. |
| Final | Catatan yang sudah diautentikasi penulisnya. Catatan final tidak bisa diubah langsung. |
| Addendum | Tambahan atau koreksi atas catatan final. Isinya alasan, isi perubahan, penulis, dan waktu. Catatan asli tetap utuh. |
| Sesi terlaksana | Sesi yang lolos verifikasi keselamatan, punya tindakan aktual, dan difinalkan pelaksananya. Status "hadir" saja **bukan** sesi terlaksana. |

**Contoh.** Pasien stroke dirujuk dari Poli Saraf. Admisi rehab mendaftarkan *permintaan*.
Dokter Sp.KFR menerimanya (*disposisi*), lalu membuka *episode*. Sp.KFR melakukan asesmen
medis dan meresepkan fisioterapi serta terapi wicara. Fisioterapis menyusun program 2× seminggu
dan terapis wicara menyusun program menelan. Delapan kunjungan berikutnya masing-masing
menghasilkan satu *sesi terlaksana* yang tertaut ke kunjungan hari itu.

---

## 3. Aktor dan Tanggung Jawab

Hak akses teknis **tidak** di-hardcode berdasarkan nama peran. Admin mengaturnya lewat layar
Pengaturan → Manajemen Role → Akses Role. Tabel di bawah adalah kewenangan **bisnis** yang
dipetakan ke aksi hak akses saat desain.

| Aktor | Boleh melakukan | Tidak boleh |
|---|---|---|
| Petugas admisi rehab | Mendaftarkan permintaan, melengkapi data, mengembalikan permintaan tidak lengkap (alasan administratif) | Menolak permintaan secara klinis, mengisi catatan klinis |
| Dokter Sp.KFR | Menelaah dan mendisposisi permintaan, asesmen medis awal, meresepkan disiplin, menghentikan atau menutup episode, keputusan lanjut/modifikasi/rujuk | Mengubah catatan profesi lain |
| Fisioterapis, terapis okupasi, terapis wicara | Asesmen profesinya, menyusun/mengubah program disiplinnya, sesi terapi, reasesmen disiplinnya | Mengaktifkan program tanpa rencana medis Sp.KFR yang final, mengubah catatan profesi lain |
| Petugas rekam medis | Membaca untuk audit, mengikuti alur koreksi RME | Mengubah isi klinis |
| Pasien/keluarga | Memberi persetujuan, menolak, menerima edukasi | — |

---

## 4. Business Rules dan Invariants

| ID | Aturan | Contoh |
|---|---|---|
| RHB-INV-01 | Permintaan wajib tertaut ke pasien dan encounter existing, atau ke dokumen rujukan eksternal. Rehab tidak membuat master pasien baru. | Permintaan untuk pasien RM 00123456 harus memilih kunjungan RJ tanggal 3 Nov 2026 yang sudah ada. |
| RHB-INV-02 | Penolakan klinis hanya oleh Sp.KFR. Admisi hanya boleh mengembalikan karena kelengkapan administratif. | Admisi boleh mengembalikan karena "surat rujukan tidak terlampir", tapi tidak boleh menolak karena "tidak ada indikasi". |
| RHB-INV-03 | Program terapi tidak bisa aktif sebelum asesmen medis Sp.KFR episode itu final (RHB-DEC-005). | Fisioterapis menyimpan draft program pada Senin. Program baru bisa diaktifkan setelah asesmen Sp.KFR difinalkan Selasa. |
| RHB-INV-04 | Sesi hanya boleh merujuk program aktif dan wajib tertaut ke encounter kunjungan hari itu (RHB-DEC-009). | Sesi ke-3 tanggal 10 Nov tertaut ke registrasi RJ 10 Nov, bukan ke registrasi permintaan 3 Nov. |
| RHB-INV-05 | Catatan final tidak bisa ditimpa. Perubahan hanya lewat addendum (RHB-DEC-007). | Terapis salah menulis durasi 45 menit (seharusnya 30). Ia membuat addendum "durasi 30 menit, salah ketik". Angka 45 tetap terlihat di riwayat. |
| RHB-INV-06 | Status tidak hadir, batal, atau ditunda tidak pernah dihitung sebagai sesi terlaksana. | Pasien tidak datang 12 Nov. Sesi itu tercatat "tidak hadir" dan tidak bisa diposting ke billing kelak. |
| RHB-INV-07 | Potensi duplikasi permintaan memunculkan peringatan, bukan penolakan otomatis. Melanjutkan wajib dengan alasan (RHB-DEC-010). | Pasien sudah punya episode fisioterapi terbuka dari RJ, lalu dirawat inap dan dirujuk lagi. Sistem menampilkan episode lama. Admisi melanjutkan dengan alasan "perubahan setting ke rawat inap". |
| RHB-INV-08 | Penulis catatan adalah pemilik catatan. Profesi lain hanya bisa membaca. | Terapis wicara tidak bisa mengubah program fisioterapi walaupun berada di episode yang sama. |
| RHB-INV-09 | Instrumen asesmen dibaca dari konfigurasi instrumen yang sudah disahkan dan tidak ditanam di kode (RHB-DEC-006). | Selama Berg Balance Scale masih berstatus konsep, formulir fisioterapi belum mewajibkannya. |
| RHB-INV-10 | Tidak ada angka frekuensi, kuota sesi, atau aturan penjamin yang di-hardcode. | Program "2× seminggu selama 4 minggu" adalah isian terapis, bukan batas sistem. |

---

## 5. State dan Transition (usulan untuk desain)

Nama status final ditetapkan saat `design-business-module` dan diselaraskan dengan enum
existing. Yang diputuskan di sini adalah **perilakunya**.

| Objek | Alur status | Siapa memicu |
|---|---|---|
| Permintaan | `Draft` → `Submitted` → `UnderReview` → `Accepted` / `NeedsClarification` / `Redirected` / `Rejected`; `Cancelled` dari `Draft` atau `Submitted` | Admisi: submit, kembalikan (administratif). Sp.KFR: telaah, terima, alihkan, tolak. |
| Episode | `Open` (menunggu asesmen Sp.KFR) → `Planning` → `Active` → `Closed` dengan alasan: selesai klinis / dihentikan / dirujuk / pindah / putus layanan / meninggal | Episode dibuka otomatis saat permintaan `Accepted`. Penutupan oleh Sp.KFR. |
| Catatan klinis (asesmen, program, sesi, reasesmen, ringkasan) | `Draft` → `Final` → (`Addendum`) | Penulisnya sendiri |
| Sesi | `Scheduled/Started` → `Completed` / `StoppedEarly` / `NotDone` (tidak hadir, batal, ditunda) | Terapis pelaksana |

**Contoh transisi yang ditolak:** permintaan `Rejected` tidak bisa langsung `Accepted`.
Pengirim harus membuat permintaan baru. Dengan begitu alasan penolakan tidak tertimpa.

---

## 6. Skenario Normal dan Exception

1. **Normal RJ berseri.** Rujukan dari poli masuk, lalu admisi mendaftarkan permintaan dan Sp.KFR
   menerimanya. Asesmen Sp.KFR difinalkan, kemudian program fisioterapi aktif. Pasien menjalani
   8 kunjungan dengan 8 sesi final. Reasesmen menghasilkan keputusan "selesai". Ringkasan akhir
   difinalkan dan episode ditutup.
2. **Permintaan tidak lengkap.** Admisi mengembalikan permintaan (`NeedsClarification`) dengan
   alasan. Pengirim melengkapi lalu submit ulang.
3. **Kondisi tidak stabil saat sesi.** Terapis menghentikan sesi (`StoppedEarly`) dan mencatat
   alasan, tindakan segera, serta pihak yang dihubungi. Program ditandai perlu telaah Sp.KFR.
4. **Pasien menolak terapi.** Penolakan dicatat bersama edukasi yang diberikan. Penolakan tidak
   menutup episode sebagai "selesai klinis".
5. **Putus layanan.** Episode ditutup dengan alasan "putus layanan", bukan "selesai". Berapa kali
   pasien harus dihubungi sebelum dianggap putus layanan masih **Open Question** RHB-OQ-005.
6. **Koreksi setelah final.** Koreksi dilakukan lewat addendum dengan mekanisme RME existing.

---

## 7. Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| RHB-FE-01 | Keamanan, privasi, invariant | Owner modul | approved | Mengikat. Tidak ada PHI di console atau storage browser, dan tidak ada CTA tanpa fungsi. | PRD §7 |
| RHB-FE-02 | Komponen dasar | Konvensi project | approved | Wajib memakai `src/components/features/base-features/*` (Hero, DataFilter, DataTable, BaseDetailView, BaseGroupedEditorView, StatusBadge, ConfirmModal, ToastStack, AccessDeniedGate). Tidak membuat ulang komponen global. | PRD §6; folder base-features terverifikasi |
| RHB-FE-03 | Menu dan route | Owner modul | draft | Menu "Rehab Medis" di bawah Health Services. Key `menuRehabMedis` sudah ada. Route final ditentukan saat desain. | `left-sidebar-menu-handle.jsx:17` |
| RHB-FE-04 | Tata letak worklist, detail, dan form | — | `DEV_DISCRETION` | Satu hero per halaman. Worklist memakai filter tanggal/periode/status/cari/refresh. Halaman dipilih alih-alih modal untuk form klinis. | PRD §5 |

---

## 8. Decision Log

Semua keputusan di bawah disetujui owner modul. Keputusan bertanda **klinis** baru boleh
diaktifkan di produksi setelah sign-off pengesah klinis (RHB-DEC-002).

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| RHB-DEC-001 | Decision | Blueprint mencakup FEAT-001..033. Rilis 1 = Phase 1. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Wawancara |
| RHB-DEC-002 | Decision | Sukma Giri Pratama adalah owner modul dan pengesah scope, teknis, serta pengajuan registry. Keputusan klinis dicatat sebagai perilaku sistem yang dapat dikonfigurasi dan wajib di-sign-off komite medik/Sp.KFR sebelum aktivasi produksi. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Wawancara |
| RHB-DEC-003 | Decision | Bentuk blueprint `SINGLE`. `shape_decided_by = USER_CONFIRMED`. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Uji pemecahan, lihat §9 |
| RHB-DEC-004 | Decision (klinis) — UNR-001 | Rilis 1 melayani Sp.KFR, fisioterapi, terapi okupasi, dan terapi wicara. Ortotik-prostetik (FEAT-011, 019–021) ditunda ke Phase 2. Daftar disiplin disimpan sebagai data konfigurasi, bukan hardcode. | Sukma Giri Pratama | approved — perlu sign-off klinis | Sukma Giri Pratama, 2026-11-05 | Wawancara |
| RHB-DEC-005 | Decision (klinis) — UNR-002 | Setiap episode wajib punya asesmen medis Sp.KFR yang final sebelum program terapi mana pun aktif. Tidak ada jalur langsung ke terapis di rilis 1. | Sukma Giri Pratama | approved — perlu sign-off klinis | Sukma Giri Pratama, 2026-11-05 | Wawancara |
| RHB-DEC-006 | Decision (klinis) — UNR-004 | Instrumen mengikuti standar RS Indonesia, dan standar internasional bila belum ada padanan Indonesia. Kandidat: **Sp.KFR/umum** NRS/VAS, Indeks Barthel; **fisioterapi** MMT 0–5, ROM goniometri, Berg Balance Scale, Timed Up and Go; **okupasi** Indeks Barthel, Lawton IADL, COPM; **wicara** TADIR, GUSS, FOIS. Semuanya dimasukkan sebagai **konsep** di mesin instrumen klinis berversi existing dan baru wajib setelah disahkan pengesah klinis di sistem. | Sukma Giri Pratama | approved — isi instrumen perlu pengesahan klinis di sistem | Sukma Giri Pratama, 2026-11-05 | `ClinicalInstrumentController` (resource `ClinicalInstrumentConfiguration`, aksi Read/Update/Approve) |
| RHB-DEC-007 | Decision (klinis) — UNR-009 | Finalisasi, autentikasi, dan addendum mengikuti mekanisme RME enterprise yang sudah ada di Quilvian. Mekanismenya diverifikasi saat trace. SLA kelengkapan ditetapkan belakangan. | Sukma Giri Pratama | approved — mekanisme perlu diverifikasi | Sukma Giri Pratama, 2026-11-05 | Wawancara |
| RHB-DEC-008 | Decision | Rilis 1: permintaan didaftarkan di modul Rehab oleh admisi atau dokter dan wajib tertaut ke encounter existing atau rujukan eksternal. Order dari layar dokter RJ/RWI/IGD menyusul sebagai slice integrasi. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Wawancara |
| RHB-DEC-009 | Decision | Satu episode mencakup banyak kunjungan. Setiap sesi tertaut ke encounter kunjungan hari itu. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Wawancara |
| RHB-DEC-010 | Decision | Potensi duplikasi memunculkan peringatan. Pengguna boleh melanjutkan dengan alasan. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Wawancara; FEAT-002 |
| RHB-DEC-011 | Decision (klinis) — UNR-003 | Sp.KFR meresepkan rencana medis dan disiplin yang dilibatkan. Tiap profesi menyusun dan mengubah program disiplinnya sendiri. Penghentian atau penutupan episode oleh Sp.KFR. Penulis adalah pemilik catatan. | Sukma Giri Pratama | approved — perlu sign-off klinis | Sukma Giri Pratama, 2026-11-05 | Wawancara |
| RHB-DEC-012 | Decision — registry | Usulan entri registry: Area `HealthServices`, modul `RehabilitationManagement / Rehabilitation`, Category `BUSINESS DOMAIN / MODULE`, prefix `Rhb` = *Rehabilitation*, lifecycle `ACTIVE` (preseden `Mdf`/`Hmd`/`Mcu`). Hanya wewenang penamaan dan kepemilikan, **bukan** wewenang migration, database, atau deployment. Diterapkan oleh RHB-DEC-014. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Prefix `Rhb` tidak ditemukan di source maupun registry (cek 2026-11-05) |
| RHB-DEC-013 | Decision (klinis) — RHB-OQ-007 | Untuk pasien rawat inap, **catatan sesi rehab adalah dokumen klinis utama** dan masuk berkas RM. CPPT cukup memuat ringkasan singkat yang merujuk sesi itu, tanpa menyalin isi klinis. Tautan otomatis CPPT ↔ sesi rehab adalah slice integrasi tersendiri yang butuh kesepakatan pemilik CPPT/Rawat Inap. Rilis 1 tidak mengubah modul CPPT. | Sukma Giri Pratama | approved — perlu sign-off klinis | Sukma Giri Pratama, 2026-11-05 | Capability map C-20 |
| RHB-DEC-014 | Decision — registry | Baris registry `HealthServices / RehabilitationManagement / Rehabilitation / Rhb / ACTIVE` ditulis ke `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (tabel utama, kepanjangan prefix, catatan perubahan lifecycle 2026-11-05). Modul tidak memakai sub-folder `MasterData` agar tidak berbenturan dengan alias `Mst`. Menutup RHB-OQ-002. Konfirmasi pemegang registry dan penyelarasan salinan suite skill belum dilakukan. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Diff registry +3 baris |
| RHB-DEC-015 | Decision — wewenang schema | Pada task backend yang disetujui roadmap, agent **boleh menulis** entity `Rhb*`, konfigurasi EF, dan **file migration**. Agent **tidak boleh** menjalankan `dotnet ef`, `dotnet build`, `dotnet test`, `database update`, maupun commit/push. Owner menjalankan build, generate/verifikasi migration, dan apply DB secara manual, lalu hasilnya dicatat di laporan task. Menutup RHB-OQ-006 dan menyelesaikan RHB-CON-001. **Risiko:** migration tulisan tangan (beserta `Designer` dan `ApplicationDbContextModelSnapshot`) bisa tidak cocok dengan model. Owner wajib memverifikasinya dengan `dotnet ef migrations add` pembanding atau `has-pending-model-changes` sebelum apply. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Wawancara |
| RHB-DEC-016 | Decision — RHB-OQ-008 | Pilihan kunjungan pada form permintaan dan sesi disediakan **endpoint lookup milik rehab** di bawah izin rehab. Hasilnya dibatasi pada pasien yang dipilih dan kunjungan yang masih terbuka (pola `HmdServiceSupport.CheckEncounterAsync`). Petugas rehab **tidak** diberi izin `PatientEncounter:Read`. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Capability map C-03 |
| RHB-DEC-017 | Decision (klinis) — RHB-OQ-009 | Rilis 1 memakai mesin instrumen untuk instrumen yang sudah bisa dimuat tipe isian dan skor `sum` existing: **NRS/VAS, Indeks Barthel, Berg Balance Scale, Timed Up and Go, Lawton IADL, GUSS, FOIS**. **COPM, MMT/ROM per sendi, dan TADIR** pada rilis 1 dicatat sebagai *pengukuran terstruktur* (nama instrumen, bagian tubuh/domain bila relevan, nilai, satuan, catatan) agar tetap bisa dibandingkan saat reasesmen. Ketiganya dipindah ke mesin instrumen setelah mesin diperluas (slice terpisah). Memperbarui cakupan RHB-DEC-006 tanpa menggantikannya. | Sukma Giri Pratama | approved — isi instrumen tetap disahkan di sistem | Sukma Giri Pratama, 2026-11-05 | Capability map C-15 |
| RHB-DEC-018 | Decision — lintas modul `Cli` | Penambahan nilai `ClinicalInstrumentKind` untuk instrumen rehab disetujui. Owner menyatakan dirinya pemilik atau sudah memperoleh persetujuan pemilik ClinicalManagement. Perluasan mesin (metode skor rata-rata, item berulang per sendi, resolusi per kode) **tidak** termasuk dan butuh keputusan terpisah. | Sukma Giri Pratama | approved — berdasarkan pernyataan owner | Sukma Giri Pratama, 2026-11-05 | Pernyataan sesi wawancara |
| RHB-DEC-019 | Decision — lintas modul `Mrc` | Penambahan jenis dokumen rehab ke `ClinicalDocumentKind` dan `JenisYangDitegakkan` milik Rekam Medis, serta pendaftaran `IMedicalRecordDocumentSource` rehab di `Program.cs`, dinyatakan **sudah disetujui** pemilik Rekam Medis (Yoga Aji Pratama). **Bukti tertulis belum dilampirkan** (lihat RHB-OQ-012). Sesuai preseden `OPT-APR-007`, task backend yang menyentuh berkas Rekam Medis wajib mengutip bukti itu sebelum ditandai ✅. | Sukma Giri Pratama | approved — bukti tertulis menyusul | Sukma Giri Pratama (atas nama pemilik RM), 2026-11-05 | Pernyataan sesi wawancara; capability map C-11..C-13 |
| RHB-DEC-020 | Decision — RHB-OQ-011 | Poli Rehabilitasi Medik (`ClinicType = Rehabilitation`, `IsDoctorRequired` sesuai kebijakan unit) dan profesi Terapis Okupasi/Terapis Wicara diinput admin data lewat layar Master Klinik dan Master Profesi yang sudah ada. Modul rehab **tidak** membuat seeder master data milik modul lain. Rehab menyimpan konfigurasi pemetaan disiplin → `MstProfession`. Kesiapan data menjadi butir checklist go-live, bukan task kode. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Capability map C-06, C-07, C-09, C-10 |
| RHB-DEC-021 | Decision — billing (gate DEC-RHB-G01) | **FEAT-032 (posting layanan selesai ke billing) ditarik ke rilis 1.** Alasannya, Billing menolak charge manual untuk pelayanan RJ/RWI (`BillingManualClinicalSourceException`, kode `RJE-VAL-010`), sehingga tanpa handoff otomatis terapi rilis 1 tidak tertagih. Polanya mengikuti Hemodialisa: setiap intervensi sesi final membentuk `TrxPatientProcedure` dari `MstProcedure` yang dipetakan ke disiplin, lalu memancarkan fakta charge lewat `ClinicalMilestoneFactProducer`. Pemetaan kode klaim JKN, SATUSEHAT, dan FEAT-033 tetap Phase 3. Butuh persetujuan pemilik Billing/ClinicalManagement atas pemakaian `TrxPatientProcedure` dan `ClinicalMilestoneFactProducer` (RHB-OQ-015). | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | `BillingInvoiceService.cs:2933`; `HmdBillingHandoffService.cs`; `HmdSessionService.cs:554` |
| RHB-DEC-022 | Decision — amandemen RHB-DEC-015 | Agent **boleh menjalankan** `ef migrations add` (lewat `ef.dll`, hanya membuat berkas migration/Designer/snapshot) dan `migrations has-pending-model-changes`. Agent juga boleh menjalankan `dotnet build` dengan perintah hemat sumber daya owner. Agent tetap **tidak** menjalankan `database update`, apply ke DB mana pun, maupun commit/push. Menggantikan larangan `dotnet ef` pada RHB-DEC-015. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 ("lanjutkan" atas opsi 1) | Sesi 2026-11-05 |
| RHB-FACT-001 | Fact | Belum ada folder `Areas/HealthServices/RehabilitationManagement` (BE) maupun `src/app/health-services/*rehab*` (FE). | — | — | — | Discovery 2026-11-05 |
| RHB-FACT-002 | Fact | Registry baris 111 menyatakan `RehabilitationManagement` tidak boleh membuat entity sebelum ada keputusan registry. | — | — | — | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:111` |
| RHB-FACT-003 | Fact | `ClinicType.Rehabilitation = 6` sudah ada, tetapi tidak dipakai di kode mana pun. | — | — | — | `Areas/HealthServices/MasterData/Enums/ClinicType.cs` |
| RHB-FACT-004 | Fact | CPPT rawat inap mendukung catatan profesi `Physiotherapist` (`CpptNoteKind.OtherProfessionNote`). | — | — | — | `PatientIntegratedProgressNoteController.cs:2214` |
| RHB-FACT-005 | Fact | Tab Rehab Medik di physician workspace rawat inap FE masih berupa panel "modul belum tersedia" (RWI-DEC-108). | — | — | — | `supporting-unavailable-panel.jsx` |
| RHB-FACT-006 | Fact | Tersedia mesin instrumen klinis berversi dengan pemisahan pengubah dan pengesah. | — | — | — | `ClinicalInstrumentController.cs`, `ClinicalInstrumentDtos.cs` |
| RHB-CON-001 | Conflict | Skill `build-module-backend` mewajibkan `dotnet build` pada task yang menyentuh source, sedangkan owner melarang agent menjalankan build. Diselesaikan RHB-DEC-015: build dijalankan manual oleh owner, dan hasilnya dicatat di laporan task sebelum task ditandai ✅. Sebelum itu, task paling tinggi berstatus 🟡. | Sukma Giri Pratama | approved | Sukma Giri Pratama, 2026-11-05 | Instruksi sesi 2026-11-05 |

---

## 9. Bentuk Blueprint

| Field | Isi |
|---|---|
| `blueprint_shape` | `SINGLE` |
| `shape_decided_by` | `USER_CONFIRMED` |

| Rumpun kemampuan | Feature | Bounded context sendiri | Kosakata status sendiri | Hak akses/pemilik peran sendiri | Rilis MVP sendiri | Master data/approval sendiri | Lolos? |
|---|---|---|---|---|---|---|---|
| Permintaan & jadwal | 001–006 | Tidak (berbagi episode) | Sebagian | Tidak | Tidak | Tidak | Tidak (≤1) |
| Rekam klinis episode | 007–010, 012–018, 024–029 | Ya | Ya | Tidak (aktor sama dengan rumpun 1) | Ya | Tidak | Tidak (2 dari 5 terpenuhi; syarat pemecahan ≥3) |
| Perangkat ortotik-prostetik | 011, 019–021 | Ya | Ya | Ya | Ya | Sebagian | Ya |
| Program rumah & edukasi | 022–023 | Tidak | Tidak | Tidak | Tidak | Tidak | Tidak |
| Handoff SATUSEHAT/billing | 030–033 | Sebagian | Ya | Ya | Ya | Tidak | Sebagian |

Hanya satu rumpun yang lolos jelas, sehingga bentuk `SINGLE` dipilih. Tidak ada kemampuan
tanpa rumpun.

---

## 10. Acceptance Criteria yang sudah dapat diuji

1. Permintaan tanpa pasien, encounter/rujukan, pengirim, indikasi, atau waktu permintaan
   ditolak BE dengan kode 400/422, dan FE menandai isian yang kurang.
2. Admisi yang mencoba aksi "tolak klinis" menerima kode 403. Aksinya terpisah dari "kembalikan".
3. Aktivasi program sebelum asesmen Sp.KFR episode itu final ditolak dengan kode 409/422 beserta
   pesan yang menjelaskan prasyaratnya.
4. Sesi tanpa program aktif atau tanpa encounter kunjungan ditolak.
5. Catatan final tidak bisa di-`PUT`/`PATCH`. Satu-satunya jalan adalah membuat addendum, dan
   catatan asli tetap terbaca.
6. Membuat permintaan untuk pasien dengan episode terbuka pada disiplin yang sama memunculkan
   peringatan. Melanjutkan tanpa alasan ditolak.
7. Formulir asesmen hanya mewajibkan instrumen yang berstatus "disahkan" di mesin instrumen.
8. Pengguna profesi A tidak bisa mengubah catatan profesi B (kode 403).
9. Sesi `NotDone` tidak pernah dihitung sebagai sesi terlaksana dalam ringkasan episode.
10. Lookup kunjungan rehab hanya mengembalikan kunjungan milik pasien yang dipilih dan masih
    terbuka. Meminta kunjungan pasien lain dengan menebak id menghasilkan kode 404, bukan data
    (RHB-DEC-016).
11. Catatan sesi rehab yang sudah ditandatangani ditolak saat diubah (lewat
    `EnsureMutableAsync`), dan addendumnya tampil di berkas rekam medis pasien (RHB-DEC-019).
12. Pasien rawat inap yang sudah pulang secara fisik tidak bisa diberi sesi baru pada kunjungan
    rawat inap itu (pola `CheckEncounterAsync`).
13. Pengukuran terstruktur (COPM, MMT/ROM, TADIR) hanya bisa dibandingkan saat reasesmen bila
    nama instrumen dan satuannya sama dengan baseline (RHB-DEC-017, FEAT-024).

---

## 11. Open Questions dan Blocker

| ID | Pertanyaan | Owner | Memblokir |
|---|---|---|---|
| RHB-OQ-001 | Siapa nama pengesah klinis (komite medik / Sp.KFR) untuk sign-off RHB-DEC-004/005/006/007/011? | Sukma Giri Pratama | Aktivasi produksi (bukan desain) |
| RHB-OQ-002 | Penerapan baris registry `Rhb`. **Ditutup** oleh RHB-DEC-014 (2026-11-05). Sisa: konfirmasi pemegang registry. | Sukma Giri Pratama | Tidak memblokir lagi |
| RHB-OQ-003 | Mekanisme finalisasi/autentikasi/addendum RME enterprise mana yang dipakai ulang? **Terjawab trace 2026-11-05:** `ClinicalDocumentIntegrityService` + `MrcClinicalNoteAddendum` (lihat `01-existing-capability-map.md` §2). Turunannya RHB-OQ-010. | Trace (`trace-existing-capabilities`) | Selesai untuk `DESIGN` |
| RHB-OQ-004 | Apakah encounter kunjungan RJ untuk sesi berseri dibuat lewat registrasi RJ existing ke poli bertipe `Rehabilitation`, dan bagaimana sesi rawat inap tertaut ke admission? **Terjawab trace 2026-11-05:** ya, pola `HmdSession` (`EncounterId` + `InpEpisodeId`), lihat `01-existing-capability-map.md` §2. Turunannya RHB-OQ-011. | Trace + owner | Selesai untuk `DESIGN` |
| RHB-OQ-005 | Definisi "putus layanan": berapa kali upaya kontak dan dalam berapa hari? (UNR-005 sebagian) | Pengesah klinis | `LATER SLICE` — rilis 1 cukup mencatat alasan penutupan |
| RHB-OQ-006 | Wewenang migration dan pembuatan tabel `Rhb*`. **Ditutup** oleh RHB-DEC-015 (2026-11-05). | Sukma Giri Pratama | Tidak memblokir lagi |
| RHB-OQ-007 | Hubungan sesi rehab rawat inap dengan CPPT. **Ditutup** oleh RHB-DEC-013. | — | — |
| RHB-OQ-008 | Sumber pilihan kunjungan. **Ditutup** oleh RHB-DEC-016. | — | — |
| RHB-OQ-009 | Cakupan instrumen rilis 1 dan pemilik `Cli`. **Ditutup** oleh RHB-DEC-017 dan RHB-DEC-018. | — | — |
| RHB-OQ-010 | Persetujuan pemilik Rekam Medis. **Ditutup** oleh RHB-DEC-019, dengan bukti menyusul (RHB-OQ-012). | — | — |
| RHB-OQ-011 | Master data poli dan profesi. **Ditutup** oleh RHB-DEC-020. | — | — |
| RHB-OQ-012 | Bukti tertulis persetujuan pemilik Rekam Medis. **Ditutup** 2026-11-06: owner mengonfirmasi Yoga Aji Pratama sudah menyetujui; dicatat di [`evidence/RHB-OQ-012-persetujuan-rekam-medis.md`](evidence/RHB-OQ-012-persetujuan-rekam-medis.md) (bentuk: pernyataan owner, belum ada artefak dari Yoga sendiri). | Sukma Giri Pratama | — |
| RHB-OQ-013 | Perluasan mesin instrumen (skor rata-rata, item per sendi, resolusi per kode) untuk COPM, MMT/ROM, dan TADIR | Pemilik ClinicalManagement | `LATER SLICE` |
| RHB-OQ-014 | Tautan otomatis CPPT ↔ sesi rehab rawat inap | Owner Rehab + pemilik CPPT/Rawat Inap | `LATER SLICE` |
| RHB-OQ-015 | Persetujuan pemilik ClinicalManagement/Billing agar rehab membuat `TrxPatientProcedure` dan memancarkan fakta charge (preseden Hemodialisa). **Ditutup** 2026-11-09: owner mengonfirmasi persetujuan; `PatientProcedureSource.RehabilitationSession = 5` (nilai baru); `DoctorId`/`InstructingDoctorId` = dokter penanggung jawab episode (Sp.KFR). Dicatat di [`evidence/RHB-OQ-015-persetujuan-clinical-billing.md`](evidence/RHB-OQ-015-persetujuan-clinical-billing.md) (bentuk: pernyataan owner, nama pemberi persetujuan belum dicatat). | Sukma Giri Pratama | — |
| RHB-OQ-016 | Kode `MstProcedure` tindakan rehab beserta tarifnya sudah ada di master tindakan/tarif? | Admin data / Billing | Go-live, bukan desain |
| UNR-005 | Aturan jadwal, prioritas, slot, daftar tunggu, keterlambatan | Owner + pengesah klinis | Phase 2 |
| UNR-006 | Aturan episode JKN, SEP/surat kontrol, frekuensi, bundling | Owner + casemix | Phase 3 |
| UNR-007 | Mapping kode tindakan, ICD-9-CM, terminologi SATUSEHAT, tarif | Owner + casemix | Phase 3 |
| UNR-008 | Profil resource dan trigger SATUSEHAT untuk rehab | Owner + admin integrasi | Phase 3 |

**Blocker desain saat ini:** tidak ada. RHB-OQ-003 dan RHB-OQ-004 terjawab oleh audit source,
sedangkan RHB-OQ-007..011 ditutup closure pass 2026-11-05.

**Masih terbuka tetapi tidak memblokir desain:** RHB-OQ-001 (nama pengesah klinis, syarat
aktivasi produksi), RHB-OQ-005 (definisi putus layanan), RHB-OQ-012 (bukti persetujuan Rekam
Medis), RHB-OQ-013, RHB-OQ-014, UNR-005..008 (Phase 2/3).
