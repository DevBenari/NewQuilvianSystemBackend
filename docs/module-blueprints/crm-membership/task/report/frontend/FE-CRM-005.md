# Laporan Perubahan Frontend — `FE-CRM-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-CRM-005` |
| Judul | Detail dan aksi |
| Slice | `MVP-2` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/frontend-roadmap.md` § `FE-CRM-005` |
| Trace | `FR-CRM-012`, `013`, `030` s.d. `035`, `062`; `CRM-DEC-013`, `021`, `022`, `024`, `029`; `CRM-FE-006`; `03-frontend-architecture.md` §1–§5 |
| Contract version | API `v2`, state `v1`, validasi `v1`, izin `v1`, semua `approved` (Yoga Aji Pratama, 2026-10-13) |
| Wewenang UI | Pakai ulang halaman detail: `CRM-FE-005`. Bentuk dialog: `DEV_DISCRETION` (`CRM-FE-006`), wajib memuat alasan (batal/verifikasi) dan tingkat tujuan (upgrade). Tata letak dan urutan tombol: `DEV_DISCRETION` (`CRM-FE-003`) |
| Dependency | `FE-CRM-001` 🟡, `BE-CRM-012` ✅, `BE-CRM-013` ✅ |
| Klasifikasi | `MEDIUM`: view detail, satu komponen dialog, hook detail ditulis ulang, dua berkas util, konstanta, pembersihan slice, satu halaman dialihkan, dua berkas lama dihapus, uji unit dan E2E |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`. Backend hanya laporan ini serta roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f1dfd3edd` (branch `YogaV2`) + perubahan `FE-CRM-001` s.d. `004`/`006` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `83ef27bd` (branch `yoga`) + perubahan `BE-CRM-012` s.d. `015` yang belum di-commit |
| Tanggal | 2026-10-14 |
| Status | Selesai. 11 skenario E2E lulus dengan API tiruan, `UAT-05`, `UAT-08`, `UAT-09`, `UAT-15` lulus terhadap backend lokal 2026-10-14 (§6.1), dan tombol per izin terbukti dengan akun petugas CRM non-administrator (`AT-24`) |

## 1. Keadaan yang ditemukan di awal

- Halaman detail (`[slug]`) masih memakai pola master data lama:
  - tombol **Ubah Data** (ke halaman ubah penuh) dan **Hapus** (dengan alasan);
  - bagian Sumber Pembuatan (kiosk/admisi/marketing) serta Poin dan Spend, yang menurut `CRM-FE-004` tidak lagi ditampilkan;
  - tanpa riwayat status, tanpa batas bayar, tanpa jenis pendaftaran.
- Halaman `[slug]/update` masih membuka formulir ubah penuh (pasien, tingkat, nomor, status, tanggal). Backend sudah memensiunkan `PUT`, `PATCH status`, dan `DELETE` (410, `BE-CRM-015`), sehingga halaman itu hanya akan gagal.
- Thunk perintah baru (perpanjangan, upgrade, batal, verifikasi, catatan, riwayat) sudah ada di slice sejak `FE-CRM-001`, tetapi belum ada layar yang memakainya.
- Bentuk data backend yang diperiksa langsung dari source:
  - enum dikirim sebagai **angka** (tanpa `JsonStringEnumConverter`): kategori batal `1` = permintaan pasien, `2` = salah input; keputusan verifikasi `1` = akui, `2` = tolak;
  - kode tindakan riwayat: `REGISTERED`, `LEGACY_FLAGGED` (dari migration), `LEGACY_ACCEPTED`, `LEGACY_REJECTED`, `CANCELLED`, `UNPAID_CANCELLED`, `EXPIRED`, `SCHEDULE_STARTED`, `NOTES_UPDATED`, `REFUND_REQUESTED`, `FEE_CHARGE_CANCEL_REQUESTED`;
  - `ApiResponse` membawa `statusCode`, sehingga 409 karena data basi dapat dibedakan dari 409 aturan bisnis lewat pesannya.

## 2. Proses bisnis dari sisi pengguna

1. Petugas CRM membuka **Daftar Membership**, lalu membuka satu membership (klik ganda atau Enter).
2. Detail menampilkan:
   - **Pasien dan Tingkat**: pasien, No. RM, kode pasien, tingkat, iuran yang dikunci saat daftar.
   - **Informasi Membership**: nomor member, status (label Indonesia), jenis pendaftaran (Baru/Perpanjangan/Upgrade/Data Lama), periode, tanggal diaktifkan, catatan.
   - **Batas Bayar** hanya untuk Menunggu Pembayaran, misalnya "20 Okt 2026 09.00" (WIB).
   - **Kategori dan Alasan Pembatalan** hanya untuk membership Dibatalkan.
   - **Riwayat Status**: waktu (WIB), tindakan, perpindahan status (misalnya "Aktif → Dibatalkan"), pelaku, alasan. Terbaru di atas. Kejadian otomatis tanpa nama pelaku tercatat "Sistem". Perubahan catatan menampilkan "Sebelumnya" dan "Menjadi".
3. Tombol di bagian atas mengikuti status dan izin:

   | Status | Tombol yang tampil (bila izinnya dimiliki) |
   | --- | --- |
   | Aktif | Perpanjang, Upgrade (`Create`), Ubah Catatan (`Update`), Batalkan (`Cancel`) |
   | Menunggu Pembayaran | Ubah Catatan, Batalkan |
   | Terjadwal | Ubah Catatan, Batalkan |
   | Perlu Verifikasi | Verifikasi (`Verify`), Ubah Catatan |
   | Kedaluwarsa, Di-upgrade, Dibatalkan | Tidak ada (hanya Kembali) |

   Tombol **Ubah Data** dan **Hapus** tidak ada lagi.
4. Setiap tombol membuka dialog:
   - **Perpanjang**: penjelasan bahwa perpanjangan adalah pendaftaran baru pada tingkat yang sama, plus catatan opsional.
   - **Upgrade**: pilihan tingkat tujuan, hanya tingkat di atas tingkat sekarang. Contoh: dari Gold hanya Platinum dan Executive yang ditawarkan, lengkap dengan iurannya.
   - **Batalkan**: pilih alasan (permintaan pasien / salah input) dan tulis keterangan minimal 10 karakter. Dialog menjelaskan akibatnya per status, misalnya untuk Aktif: "Tanpa refund. Bila pasien punya perpanjangan Terjadwal, perpanjangan itu ikut dibatalkan dan refund-nya diminta."
   - **Verifikasi**: pilih Akui (isi tanggal mulai dan akhir, keterangan opsional) atau Tolak (alasan minimal 10 karakter).
   - **Ubah Catatan**: catatan sekarang sudah terisi dan bisa diganti (maksimal 250 karakter).
5. Setelah berhasil:
   - dialog tertutup, notifikasi tampil, lalu detail dan riwayat dimuat ulang;
   - Perpanjang/Upgrade menampilkan kartu "Perpanjangan terdaftar: MBR-…" berisi status, tingkat, iuran, batas bayar, dan tombol **Buka Pendaftaran Baru**. Membership yang sedang dibuka tidak berubah.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Isian wajib kosong atau alasan terlalu pendek | Pesan di bawah isian, permintaan tidak dikirim. Contoh: "Tulis alasan pembatalan minimal 10 karakter." |
| Backend menolak (400/409 aturan bisnis) | Pesan backend tampil apa adanya di dalam dialog, dialog tetap terbuka. Contoh `UAT-05`: "Perpanjangan baru bisa mulai 12 Sep 2027." |
| Data sudah diubah petugas lain (409 `RowVersion`) | Dialog tertutup, notifikasi "Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi.", detail dan riwayat dimuat ulang otomatis |
| Batal Aktif karena salah input | Backend menolak (fail-closed sampai modul kasir siap). Pesannya tampil apa adanya di dialog |
| Selama permintaan berjalan | Tombol Simpan dan Batal di dialog terkunci, tombol aksi di atas terkunci |
| Tautan lama `…/[slug]/update` | Dialihkan ke halaman detail |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`, `test-policy.md`, `REPORT_TEMPLATE.md`; halaman `[slug]`, `[slug]/update`, `create`; view detail, form view, hook detail dan editor, slice, konstanta, util tampilan/daftar/pendaftaran; `health-services-master-data-detail-view.jsx`, `base-detail-view.jsx`, `hero.jsx`, `confirm-modal.jsx`, `base-grouped-editor-field.jsx`, `filter-date-picker.jsx`, `use-permission.jsx`; acuan pola `journal-detail-view.jsx` dan `journal-reversal-dialog.jsx` (tombol aksi di hero + `ConfirmModal` dengan isian); pola redirect `petty-cash-category/[slug]/update/page.jsx`. Backend: `PatientMembershipDtos.cs`, `PatientMembershipController.cs`, `PatientMembershipService.cs` (`CancelAsync`, `VerifyLegacyAsync`, kode riwayat), enum membership, `Program.cs` (opsi JSON), `MembershipTierDtos.cs`, migration `ExtendPatientMembershipLifecycle`. Dokumen: roadmap, kontrak API/state/validasi, `03-frontend-architecture.md`, laporan `BE-CRM-012`/`013`, `FE-CRM-004`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/.../patient-memberships/detail/patient-memberships-detail-view.jsx` | Ditulis ulang: tombol aksi lewat `renderHero` + `Hero` `renderDetailActions`, kartu pendaftaran baru, kartu Riwayat Status (tabel, kosong, gagal + Coba Lagi), dialog aksi |
| `src/components/view/.../patient-memberships/detail/patient-membership-action-dialog.jsx` | Baru. Satu dialog `ConfirmModal` untuk lima aksi, isian memakai `BaseGroupedEditorField` (pilihan, tanggal, teks panjang) |
| `src/lib/hooks/.../patient-memberships/use-master-data-patient-membership-detail.jsx` | Ditulis ulang: muat detail + riwayat, empat `usePermission`, keadaan dialog, validasi, kirim perintah, muat ulang setelah berhasil/409 basi. Logika hapus, ubah status, dan ke halaman ubah dihapus |
| `src/utils/.../patient-membership/patient-membership-action-utils.jsx` | Baru. Tombol per status dan izin, form awal, validasi (`VAL-10`, `11`, `14` urutan tanggal, `16`), body perintah, pilihan tingkat upgrade, deteksi 409 basi, keterangan akibat batal |
| `src/utils/.../patient-membership/patient-membership-detail-utils.jsx` | Baris detail mendukung format status, jenis pendaftaran, periode, WIB, kategori batal, dan baris khusus status. Normalisasi riwayat (urut terbaru, label, pelaku "Sistem"/"Kasir", perubahan catatan) |
| `src/lib/constants/.../patient-membership-constants.jsx` | Seksi detail siklus hidup (tanpa sumber pembuatan, poin, spend). Konstanta aksi, label kategori batal, keputusan verifikasi, sumber perubahan, kode riwayat, batas alasan 10–500, keterangan batal. Konstanta formulir ubah lama dihapus |
| `src/lib/state/slice/.../master-data-patient-membership-slice.jsx` | Thunk `updatePatientMembership` (`PUT`), `deletePatientMembership` (`DELETE`), `updatePatientMembershipStatus` (`PATCH status`) yang dipensiunkan backend dihapus, sesuai catatan `FE-CRM-001`. `getPatientMembershipMasterOptions` dan state `masterOptions` (hanya dipakai editor lama) dihapus |
| `src/app/.../patient-memberships/[slug]/update/page.jsx` | Dialihkan (`redirect`) ke detail |
| `src/components/view/.../patient-memberships/add/patient-memberships-form-view.jsx` | Hanya merender alur pendaftaran; mode ubah dihapus |
| `src/lib/hooks/.../use-master-data-patient-membership-editor.jsx` | **Dihapus** (hanya dipakai halaman ubah lama) |
| `src/utils/.../patient-membership/patient-membership-editor-utils.jsx` | **Dihapus** (hanya dipakai hook editor) |
| `src/style/health-services/patient-management/patient-membership-detail.module.css` | Baru. Kartu, tabel riwayat (geser horizontal di ponsel), isi dialog, dua kolom tanggal yang menjadi satu kolom di ponsel |
| `src/utils/.../patient-membership/patient-membership-display-utils.jsx` | `createToast` mengisi `variant` agar `ToastStack` memberi warna galat/sukses (temuan UAT §6.1) |
| `tests/unit/crm-membership-detail.test.mjs` | Baru, 13 uji (termasuk uji `variant` notifikasi) |
| `tests/unit/crm-membership-slice-admin-routes.test.mjs` | Dua uji `FE-CRM-001` yang memakai thunk lama diganti: pilihan tingkat lewat `getRegistrationMembershipTierOptions`; slice tanpa `PUT`/`DELETE`/`/status`/`masterOptions` dan pesan 409 perintah diteruskan |
| `tests/unit/crm-membership-registration.test.mjs`, `crm-membership-list.test.mjs` | Pemeriksaan disesuaikan dengan form view tanpa mode ubah dan kunci baris status detail yang baru |
| `tests/e2e/crm-membership-detail-screen.spec.mjs` | Baru, 11 skenario dengan API tiruan berkeadaan |

Semua berkas sumber yang ditulis memakai CRLF seperti tetangganya. Berkas uji LF.

### 3.3 Kepatuhan arsitektur frontend

Alur tetap `page → view → hook → slice → InstanceAxios`. View tidak memanggil Axios, dialog tidak memanggil API, dan transformasi berada di util murni yang diuji unit. Tidak ada komponen base baru maupun perubahan pada komponen base: `HealthServicesMasterDataDetailView` dipakai apa adanya dengan `renderHero`, dan `ConfirmModal` dipakai dengan `children` seperti dialog koreksi jurnal. Isian dialog memakai `BaseGroupedEditorField`. Token `--group-*` disediakan dengan membungkus isian dengan kelas `groupedEditorPage` dari modul CSS base yang sama.

Penyimpangan dari aturan umum `AGENTS.md` ("aksi detail master data = Kembali, Perbarui, Hapus") disengaja dan berdasar keputusan owner: ubah penuh dilarang (`CRM-DEC-022`) dan riwayat tidak boleh dihapus (`CRM-DEC-013`).

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil detail membership..." pada kartu. Riwayat: "Mengambil riwayat status...". Tombol aksi terkunci |
| Kosong | Riwayat: "Belum ada perubahan yang tercatat." Detail tidak ada: "Data membership tidak ditemukan." |
| Gagal | Detail: pesan backend di alert merah. Riwayat: pesan backend + tombol **Coba Lagi**. Perintah: pesan backend di dialog |
| Tanpa hak akses | Tombol yang izinnya tidak dimiliki tidak tampil. Pengguna hanya `Read` melihat detail dan riwayat dengan tombol Kembali saja. Backend tetap penjaga akhir (403) |
| Data basi | 409 `RowVersion` → notifikasi + muat ulang otomatis |
| Kirim ganda | Tombol di dialog dan di atas terkunci selama permintaan |
| Privasi | Alasan pembatalan dan catatan hanya di detail, tidak di daftar |
| Aksesibilitas | Isian berlabel (terbaca pembaca layar lewat `label`), pesan galat terhubung `aria-describedby`, kartu pendaftaran baru `role="status"`. Tombol bisa dioperasikan papan ketik |
| Ponsel (390 px) | Tombol aksi bertumpuk, tabel riwayat bisa digeser, tanggal di dialog satu kolom. Seluruh 11 skenario E2E juga lulus di lebar 390 px |

## 5. Endpoint yang dikonsumsi

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patient-memberships/admin/{id}` | Detail | `PatientMembership : Read` |
| `GET` | `/v1/.../patient-memberships/admin/{id}/status-histories` | Riwayat status | `PatientMembership : Read` |
| `POST` | `/v1/.../patient-memberships/admin/{id}/renewals` | Perpanjang: `{ notes, expectedRowVersion }` | `PatientMembership : Create` |
| `POST` | `/v1/.../patient-memberships/admin/{id}/upgrades` | Upgrade: `{ membershipTierId, notes, expectedRowVersion }` | `PatientMembership : Create` |
| `POST` | `/v1/.../patient-memberships/admin/{id}/cancel` | Batalkan: `{ category: 1/2, reason, expectedRowVersion }` | `PatientMembership : Cancel` |
| `POST` | `/v1/.../patient-memberships/admin/{id}/verify` | Akui `{ decision: 1, periodStartDate, periodEndDate, reason?, expectedRowVersion }` / tolak `{ decision: 2, reason, expectedRowVersion }` | `PatientMembership : Verify` |
| `PATCH` | `/v1/.../patient-memberships/admin/{id}/notes` | Ubah catatan: `{ notes, expectedRowVersion }` | `PatientMembership : Update` |

#### Administrator / Master Data / Membership Tier

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/administrator/master-data/membership-tiers/options?onlyActive=true&registrableOnly=true` | Pilihan tingkat upgrade (disaring `PriorityLevel` di atas tingkat sekarang) | `MembershipTier : Read` |

Endpoint yang **berhenti** dipakai: `PUT /admin/{id}`, `PATCH /admin/{id}/status`, `DELETE /admin/{id}`, dan `GET /v1/.../patients/options` (daftar pasien untuk editor lama).

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji unit `crm-membership-detail.test.mjs`: tombol per status dan izin; batas bayar hanya Menunggu; tanpa poin/spend/sumber; kategori/alasan batal hanya Dibatalkan; `UAT-09` di layar; validasi verifikasi; body lima perintah (enum angka + `expectedRowVersion`); upgrade dari Gold hanya Platinum/Executive; deteksi 409 basi; keterangan batal; riwayat (urut, label, "Sistem"/"Kasir", catatan); halaman ubah dialihkan dan editor terhapus | 12 lulus | `PASS` | Keluaran `node --test` |
| Seluruh uji CRM (`tests/unit/crm-membership-*.test.mjs`) | 48 lulus | `PASS` | Keluaran `node --test` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2.848 uji, 2.819 lulus, 29 gagal | `EXISTING / ENVIRONMENT ISSUE` (29) | Jumlah gagal sama dengan baseline (29). Seluruhnya non-CRM (lab, hemodialisa, rawat inap, rawat jalan, bank darah, sidebar keuangan). 2.836 sebelumnya + 12 uji baru = 2.848 |
| `npx eslint` berkas membership yang disentuh | Bersih. Satu peringatan `preserve-manual-memoization` pada hook detail ikut diperbaiki. Peringatan `set-state-in-effect` di `use-patient-membership-registration.jsx` sudah ada sejak `FE-CRM-003`, tidak disentuh | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Bersih | `PASS` | Keluaran perintah |
| `npm run build` | "Compiled successfully in 77s". Rute `[slug]` dan `[slug]/update` terdaftar | `PASS` | Log build |
| E2E detail Aktif: data siklus hidup, riwayat "Didaftarkan / Rina CRM", tombol Perpanjang/Upgrade/Ubah Catatan/Batalkan, tanpa Verifikasi/Ubah Data/Hapus, tanpa Batas Bayar/Saldo Poin/Total Spend/Dibuat dari Kiosk | Lulus | `PASS` | `crm-membership-detail-screen.spec.mjs` + tangkapan layar |
| E2E hanya `Read` → hanya tombol Kembali | Lulus | `PASS` | idem |
| E2E `UAT-05`: Perpanjang → 409 "Perpanjangan baru bisa mulai 12 Sep 2027." tampil di dialog. Body `{ notes: null, expectedRowVersion }` | Lulus | `PASS` | idem |
| E2E perpanjang berhasil → kartu "Perpanjangan terdaftar: MBR-000010", batas bayar "8 Okt 2027 09.00", detail dan riwayat dimuat ulang | Lulus | `PASS` | idem |
| E2E upgrade: tanpa tingkat ditolak di layar; pilihan hanya Platinum/Executive; body membawa Platinum | Lulus | `PASS` | idem |
| E2E `UAT-09`: batal tanpa alasan dan kategori → dua pesan, tanpa permintaan. Dengan alasan → Dibatalkan, kategori + alasan tampil, tombol aksi hilang, riwayat "Aktif → Dibatalkan" | Lulus | `PASS` | idem |
| E2E `UAT-15`: 409 "baru saja diubah petugas lain" → dialog tertutup, notifikasi, detail dimuat ulang dan menampilkan keadaan terbaru (sudah Dibatalkan) | Lulus | `PASS` | idem |
| E2E Menunggu Pembayaran: "Batas Bayar 20 Okt 2026 09.00", tombol Ubah Catatan/Batalkan saja; ubah catatan → `PATCH /notes`, riwayat "Menjadi: Telepon ulang Senin" | Lulus | `PASS` | idem |
| E2E `UAT-08`: Verifikasi → Akui tanpa periode ditolak; tanggal dipilih lewat kalender → body `decision: 1` + tanggal; status Aktif, riwayat "Data lama diakui" | Lulus | `PASS` | idem |
| E2E tolak data lama: tanpa alasan ditolak; dengan alasan → body `{ decision: 2, reason, expectedRowVersion }` | Lulus | `PASS` | idem |
| E2E `…/update` dialihkan ke detail | Lulus | `PASS` | idem |
| Seluruh 11 skenario di lebar 390 px (sementara, berkas uji dihapus sesudahnya) | Lulus. Tangkapan layar diperiksa | `PASS` | Keluaran Playwright |

Uji manual: `PASS` — kontrol interaktif (lima dialog, pilihan kategori/keputusan/tingkat, kalender di dalam dialog, kunci tombol, galat isian, galat backend, muat ulang) dijalankan lewat Playwright di server standalone dan diperiksa pada tangkapan layar desktop dan ponsel.

Server standalone 3710 dinyalakan untuk E2E lalu dimatikan. `test-results/` tidak berubah. Keluaran Playwright ditulis ke scratchpad.

**Tidak dijalankan pada pengerjaan awal:** UAT terhadap backend sungguhan. Sudah dijalankan kemudian, lihat §6.1.

### 6.1 UAT terhadap backend lokal (2026-10-14)

Dijalankan atas instruksi owner (2026-10-14): nominal iuran contoh, menulis data uji lewat aplikasi, dan menyemai data lama lewat SQL. Bagian ini menjadi rujukan UAT untuk `FE-CRM-003`, `004`, `005`, dan `007`.

**Lingkungan:**

| Komponen | Keadaan |
| --- | --- |
| Backend | Branch `yoga` `83ef27bd` + perubahan `BE-CRM-012` s.d. `015` (belum di-commit). Build 13 Okt 15.58, lebih baru dari seluruh source. Dijalankan `ASPNETCORE_ENVIRONMENT=Development` di http 5107, `PatientMembership__Lifecycle__Enabled=false` agar proses harian CRM tidak menulis selama uji |
| Database | `QuilvianNewDevYoga`. Tertunda 3 migration milik modul lain (HR, penjamin perusahaan pasien, piutang); tidak ada yang CRM |
| Frontend | Build standalone 3710 dari working tree ini. Panggilan `/v1/**` diteruskan Playwright ke backend lokal dengan sesi **superadmin** |
| Akun | Superadmin pengganti peran petugas CRM. Belum ada departemen-posisi dengan `PatientMembership : Cancel`/`Verify` |

**Data yang ditulis ke devYoga:**

| Data | Cara | Isi |
| --- | --- | --- |
| Iuran tingkat (nilai contoh, `CRM-OQ-001` belum diputuskan) | Layar Master Tingkat Membership (`PUT` 200 ×4) | Silver 1.200.000, Gold 2.400.000, Platinum 5.000.000, Executive 9.000.000 per 12 bulan. Regular tetap 0 |
| Tiga membership data lama | SQL semai, uji kering + `ROLLBACK` lalu `COMMIT`, meniru migration `AddPatientMembershipStatusUniqueIndexes` | `UAT-LAMA-001` ANDRY ZAINUDIN Gold, `UAT-LAMA-002` AGNES YULIANI RAJA GUK GUK Silver, `UAT-LAMA-003` SHANAYA ALMEERA PUTRI MAHESWARI Gold. Ketiganya Perlu Verifikasi, jenis Data Lama, satu riwayat `LEGACY_FLAGGED` masing-masing; `IsMember` ketiga pasien dikosongkan seperti langkah 2 migration |
| Hasil UAT | Layar aplikasi | `MBR-000001` (upgrade Platinum ANDRY, kemudian dibatalkan), `MBR-000002` (daftar baru Gold IKBAL YULIYANTO, **Menunggu Pembayaran** sampai 21 Okt 2026 11.14, akan dibatalkan proses harian bila tidak dibayar), pasien baru IGD `UAT FECRM PASIEN IGD` RM 00-00-00-17, simpan ulang data IKBAL di master pasien |

**Hasil:**

| UAT / skenario | Hasil di layar | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `UAT-01` filter Perlu Verifikasi | 3 baris `UAT-LAMA-001`…`003`, jenis Data Lama, status Perlu Verifikasi | `PASS` | Tangkapan layar + `GET /admin?membershipStatus=9` |
| `UAT-08` akui `UAT-LAMA-001` dengan periode 1 Agu 2026 – 30 Apr 2027 (dipilih lewat kalender) | Aktif, riwayat "Data lama diakui", periode tampil | `PASS` | `POST /verify` 200, body `decision: 1` + tanggal |
| `UAT-05` perpanjang `UAT-LAMA-001` (berakhir 30 Apr 2027) | Dialog menampilkan "Perpanjangan baru bisa mulai 31 Mar 2027." | `PASS` | `POST /renewals` 409 |
| `UAT-04` daftar baru Silver untuk ANDRY yang masih Gold Aktif | Hasil cari menandai "Member Aktif — Gold Member"; simpan → "Pasien sudah member Gold Member sampai 30 Apr 2027. Gunakan perpanjangan atau upgrade." | `PASS` | `POST /admin` 409 |
| Upgrade Gold → Platinum | Kartu "Upgrade terdaftar: MBR-000001"; DB: Menunggu Pembayaran, jenis Upgrade, iuran 5.000.000, batas bayar 21 Okt 2026 11.13, menunjuk `UAT-LAMA-001` | `PASS` | `POST /upgrades` 200 + kueri baca-saja. Daftar pilihan tingkat tidak tercatat oleh skrip (galat parse skrip, bukan aplikasi); penyaringan diuji unit dan E2E tiruan |
| `UAT-09` batal tanpa alasan, lalu alasan "pendek" | Dua pesan isian, nol permintaan `cancel` terkirim | `PASS` | Log permintaan |
| `UAT-15` dua halaman membuka `UAT-LAMA-001` lalu sama-sama membatalkan | Petugas A 200; petugas B mendapat "Data ini baru saja diubah petugas lain…", detail B dimuat ulang menjadi Dibatalkan, tombol Batalkan hilang. DB: hanya satu riwayat `CANCELLED` | `PASS` | `POST /cancel` 200 lalu 409 + kueri baca-saja |
| Detail Menunggu Pembayaran `MBR-000001` | Batas bayar "21 Okt 2026 11.13"; tombol hanya Ubah Catatan dan Batalkan | `PASS` | Tangkapan layar |
| Ubah catatan | Dialog terisi catatan lama; riwayat "Menjadi: UAT FE-CRM: hubungi pasien sebelum batas bayar" | `PASS` | `PATCH /notes` 200 |
| Batal Menunggu Pembayaran, kategori Salah input | Petunjuk "Tagihan iuran … diminta untuk dibatalkan"; riwayat "Pembatalan tagihan iuran diminta" | `PASS` | `POST /cancel` 200 |
| Tolak `UAT-LAMA-002` | Tanpa alasan ditolak di layar; dengan alasan → Dibatalkan, riwayat "Data lama ditolak" | `PASS` | `POST /verify` 200, `decision: 2` |
| Akui `UAT-LAMA-003` dengan periode lewat (1 Okt 2025 – 30 Sep 2026) | Kedaluwarsa, tanpa tombol aksi | `PASS` | `POST /verify` 200 |
| `UAT-03` daftar baru Gold untuk IKBAL YULIYANTO | Panel "Membership MBR-000002 terdaftar — Menunggu Pembayaran", iuran Rp 2.400.000, batas bayar 21 Okt 2026 11.14 | `PASS` | `POST /admin` 200 |
| `UAT-11` (a) simpan ubah pasien IKBAL di master pasien | Tanpa isian member di layar; `PUT` tanpa `isMember`/tier/membership aktif; DB `IsMember=false` | `PASS` | `PUT /patients/{id}` 200 + kueri baca-saja |
| `UAT-11` (b) pasien baru lewat IGD | Tanpa kartu member; tersimpan dan lanjut ke langkah 2; DB `IsMember=false` (sebelum `BE-CRM-015` selalu `true`) | `PASS` | `POST /patients` 200 + kueri baca-saja |
| Keadaan akhir DB | Riwayat: `LEGACY_FLAGGED` 3, `LEGACY_ACCEPTED` 2, `LEGACY_REJECTED` 1, `CANCELLED` 2, `FEE_CHARGE_CANCEL_REQUESTED` 1, `NOTES_UPDATED` 1, `REGISTERED` 2. Kelima pasien uji `IsMember=false` (tidak ada yang Aktif) | `PASS` | Kueri baca-saja |

**Temuan dan perbaikannya:** notifikasi membership (sukses maupun galat) selalu tampil sebagai info biru, karena `createToast` di `patient-membership-display-utils.jsx` hanya mengisi `type`, padahal `ToastStack` membaca `variant`. Diperbaiki dengan menambah `variant: type` (satu baris, ikut berlaku untuk layar pendaftaran `FE-CRM-003`) dan satu uji unit. Sesudah build ulang, notifikasi "Data sudah berubah" tampil merah. Regresi: 28 skenario E2E CRM lulus, 54 uji unit CRM lulus, suite penuh 2.854 uji dengan 29 gagal yang daftarnya identik dengan baseline, `lint:errors` dan build bersih.

**Susulan `AT-24` (2026-10-14):** sesudah jabatan Pendaftaran / Customer Service diberi izin CRM, akun `citra.amelia@rsmmc.local` (bukan administrator) membuka detail Menunggu Pembayaran dan mendapat tombol Ubah Catatan + Batalkan. Ubah catatan berhasil dengan pelaku tercatat atas namanya. Detail Kedaluwarsa tanpa tombol, dan pendaftaran `MBR-000003` (Silver, Indra Gunawan) berhasil. Tidak ada 403. Rincian dan perbaikan 404 rute tingkat lama ada di [`FE-CRM-001.md` §6.1](FE-CRM-001.md#61-at-24--uat-14-dengan-akun-petugas-crm-sungguhan-2026-10-14).

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Detail menampilkan riwayat status | Terpenuhi | E2E detail Aktif, `UAT-09`, ubah catatan |
| Tombol sesuai status dan izin: Perpanjang (dalam jendela), Upgrade, Batalkan (kategori + alasan), Verifikasi (akui dengan periode / tolak dengan alasan), Ubah catatan | Terpenuhi. Perpanjang tampil untuk semua membership Aktif; jendela 30 hari dijaga backend dan pesannya tampil apa adanya, sesuai `UAT-05` | Uji unit tombol + E2E |
| Halaman `[slug]/update` dihapus atau dialihkan ke detail | Terpenuhi (dialihkan) | E2E redirect + uji unit |
| `UAT-05`, `UAT-08`, `UAT-09`, `UAT-15` lulus lewat UI | Terpenuhi: lulus dengan API tiruan dan terhadap backend lokal (superadmin) | E2E + §6.1 |
| Setelah aksi berhasil, detail dimuat ulang | Terpenuhi (detail dan riwayat) | E2E perpanjang, batal, catatan, verifikasi |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Petugas CRM butuh izin baru `PatientMembership : Cancel` dan `PatientMembership : Verify` di Role Access, serta `MembershipTier : Read` untuk pilihan upgrade. Tanpa izin itu tombolnya tidak tampil |
| Masalah yang diketahui | (1) Batal Aktif dengan kategori "Salah input" selalu ditolak backend sampai modul kasir siap (`BE-CRM-013`, fail-closed). Pilihannya tetap ada, dan pesan backend menjelaskan alasannya. (2) Label kecil di hero tetap "Master Data" karena tertanam di `HealthServicesMasterDataDetailView` yang dipakai banyak modul. (3) Isi dialog kosong sesaat selama animasi menutup (±150 ms) |
| Dependency backend | `BE-CRM-012` ✅, `BE-CRM-013` ✅. `FE-CRM-001` 🟡 tetap dirilis bersama `BE-CRM-015` |
| Perubahan sampingan | `NONE`. Build menulis `.next/standalone`. Berkas spec sementara lebar ponsel dibuat lalu dihapus |
| Interupsi | `NONE` |
| Status Git | ` M` view detail, hook detail, slice, konstanta, detail utils, halaman update, form view (beserta perubahan `FE-CRM-001` s.d. `004`/`006` sebelumnya). ` D` `use-master-data-patient-membership-editor.jsx`, `patient-membership-editor-utils.jsx`. `??` `patient-membership-action-dialog.jsx`, `patient-membership-action-utils.jsx`, `patient-membership-detail.module.css`, `crm-membership-detail.test.mjs`, `crm-membership-detail-screen.spec.mjs`. Belum di-commit |
| Langkah berikutnya | Ganti iuran contoh dengan nominal resmi (`CRM-OQ-001`) sebelum go-live. Commit dan rilis bersama `BE-CRM-015` + `FE-CRM-001` |
