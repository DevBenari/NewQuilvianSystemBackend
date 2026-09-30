# Checklist Verifikasi Modernisasi Menu

Checklist ini khusus skill pribadi `modernisasi-menu-v1`. Isinya hanya pemeriksaan yang khas untuk
modernisasi menu klinis. Pemeriksaan umum tetap dibaca langsung dari `rules/` dan **tidak disalin
ke sini**:

| Pemeriksaan umum | Sumber canonical |
| --- | --- |
| Konsistensi UI dan grep anti-regresi (warna literal, typography, tombol, tabel, `!important`) | `rules/frontend/ui-consistency-checklist.md` bagian G dan H |
| Gerbang base component | `rules/frontend/base-component-decision-gate.md` |
| Review diff, scope, rahasia, dan klasifikasi validasi | `rules/backend/REVIEW_RULES.md` |
| Kewajiban test | `rules/backend/TEST_POLICY.md`, `rules/frontend/test-policy.md` |
| Penandaan roadmap | `rules/rule-output/status-task-roadmap.md` |
| Checklist dokumen | `rules/rule-output/aturan-output-dokumentasi.md` bagian *Checklist* |

Checklist ini dipakai di dua tahap:

- **Tahap 2**: bagian A, saat menyusun kontrak di dokumen rencana kerja.
- **Tahap 5**: seluruh bagian.

Setiap butir dijawab `PASS`, `FAIL`, atau `N/A — <alasan>`. Jangan menulis `PASS` untuk sesuatu
yang tidak dijalankan.

---

## A. Kontrak Field dan Transisi FE ↔ BE

Butir A1–A9 berasal dari blocker nyata di `rawat-inap/keperawatan/testing/issues/`, yang semuanya
lolos build. Butir A10–A14 berasal dari `rules/backend/transaction-endpoint-standard.md` dan
`rules/backend/role-access-rules.md`.

| No | Periksa | Asal |
| --- | --- | --- |
| A1 | Nama properti JSON di payload/hook sama persis dengan DTO (camelCase), termasuk ID konteks `patientId` / `encounterId` | ISSUE-KEP-006, ISSUE-KEP-007 (`versionId` vs `instrumentVersionId`) |
| A2 | Enum dikirim dalam bentuk yang diterima server (angka atau teks), dan konversi di FE tidak bisa menghasilkan `NaN`/`null` | ISSUE-KEP-001 |
| A3 | Field enum/angka yang tidak boleh kosong punya kontrol input wajib atau nilai default di form | ISSUE-KEP-008 (`ConsciousnessStatus`) |
| A4 | Respons berpaginasi dibaca dari `items` (`PagedResult<T>`), bukan dianggap array | ISSUE-KEP-003 |
| A5 | Nilai status dokumen (draf/berjalan/selesai) di FE sama dengan enum server | ISSUE-KEP-003 |
| A6 | Butir instrumen yang punya `binding` ke kolom entitas tidak ikut dikirim di `responses` JSON | ISSUE-KEP-004 |
| A7 | Hak akses endpoint mengizinkan pengguna menu, termasuk syarat akun tertaut `MstEmployee` | ISSUE-KEP-002, ISSUE-KEP-008 |
| A8 | Syarat status versi instrumen (Approved vs Draft) serta penanganan `422` dan `409 INSTRUMENT_VERSION_CHANGED` di UI jelas | ISSUE-KEP-003, ISSUE-KEP-007 |
| A9 | Tab/navigasi baru terdaftar di peta tab; tidak jatuh ke formulir fallback | ISSUE-KEP-005 |
| A10 | Tombol aksi (Simpan, Selesaikan & Kunci, Koreksi) ditampilkan dari `AvailableActions` backend, bukan ditebak dari status | standar transaksi 5.1 |
| A11 | Transisi yang tidak sah dijawab `400`/`403`/`404`/`409` dengan pesan terbaca, tidak pernah `200` berisi pesan gagal | standar transaksi 5.2 |
| A12 | Aksi klinis yang tidak bisa dibatalkan (mis. mengunci pengkajian) menerima `IdempotencyKey` | standar transaksi 5.3 |
| A13 | Setiap perpindahan status mencatat siapa, kapan, dari/ke status, dan alasan bila dibutuhkan | standar transaksi 5.4, `QBE-LOG-001` |
| A14 | Kewenangan memakai `[AccessAction]`/`[AccessPermission]`, bukan nama peran atau departemen di kode | role-access-rules bagian 5, `QBE-PERM-001` |

---

## B. Anti-Hardcode Logika Klinis

Grep UI umum dijalankan dari `ui-consistency-checklist.md` bagian G. Bagian ini hanya menambahkan
pemeriksaan logika klinis yang tidak tercakup di sana (`RWI-DEC-136`).

Jalankan dari root workspace pada **berkas yang diubah task**. Bila perlu, tambahkan folder fitur
menu itu. Lalu klasifikasikan setiap hasil sebagai `OK — <alasan>` atau `TEMUAN`.

```bash
FE_REPO="QuilvianFinal/QuilvianSystemFrontendDev"
BE_REPO="QuilvianFinal/NewQuilvianSystemBackend"
# berkas yang diubah (read-only)
FE_FILES=$(git -C "$FE_REPO" diff --name-only HEAD -- '*.js' '*.jsx' 2>/dev/null | sed "s#^#$FE_REPO/#")
BE_FILES=$(git -C "$BE_REPO" diff --name-only HEAD -- '*.cs' ':!Migrations/**' 2>/dev/null | sed "s#^#$BE_REPO/#")

# 1. Ambang skor/band klinis tertanam di frontend
grep -nE "(score|skor|total|\bs)[[:space:]]*>=?[[:space:]]*[0-9]+" $FE_FILES /dev/null

# 2. Daftar opsi/intervensi klinis tertanam di frontend
grep -nE "\{[[:space:]]*code:[[:space:]]*\"[A-Z_]+\"[[:space:]]*,[[:space:]]*label:" $FE_FILES /dev/null

# 3. Tanda tangan / nama petugas statis
grep -niE "ttd|signature|\.png\"|\.jpg\"" $FE_FILES /dev/null

# 4. Kewenangan berbasis nama peran di backend
grep -nE "IsInRole\(|Roles[[:space:]]*=|^[[:space:]]*\"(SuperAdmin|Supervisor|KepalaRuangan|Perawat|Nurse|Dokter|Doctor|Kasir|Farmasi)\",?[[:space:]]*$|==[[:space:]]*\"(Perawat|Nurse|Dokter|Doctor|KepalaRuangan|Kasir|Farmasi|SuperAdmin)\"" $BE_FILES /dev/null
```

Untuk berkas baru yang belum di-track git, tambahkan path-nya secara manual. Tidak ada keluaran
berarti tidak ada temuan (`grep` keluar dengan kode 1, dan itu normal).

Pola 4 sudah dikalibrasi pada anti-pola canonical `InpatientActorClaims.cs` (menangkap
`SupervisorOrWardHeadRoles` dan isinya), tanpa false positive pada
`Migrations/ApplicationDbContextModelSnapshot.cs`. Temuan pada kode lama yang tidak disentuh task
hanya dicatat, tidak diperbaiki (role-access-rules bagian 5).

Kalibrasi 30 September 2026 pada folder komponen resiko-jatuh: perintah 1 menemukan 5 baris dan
perintah 2 menemukan 33 item. Keduanya temuan yang belum diperbaiki.

---

## C. Perintah Validasi

| Repo | Perintah | Dari folder | Catatan |
| --- | --- | --- | --- |
| Backend | `dotnet build QuilvianSystemBackend.csproj --no-incremental` | `QuilvianFinal/NewQuilvianSystemBackend` | Backend tidak punya project test. Jangan menjalankan `dotnet test` dan jangan membuat project/framework test tanpa permintaan eksplisit |
| Frontend | `npm run lint:errors` | `QuilvianFinal/QuilvianSystemFrontendDev` | Validasi minimum `AGENTS.md` frontend |
| Frontend | `npm run test:unit` | sama | Menjalankan suite yang ada; menulis test baru opsional; jangan menambah Jest |
| Frontend | `npm run build` | sama | Bila terhalang lock `.next`, laporkan apa adanya |
| Frontend (opsional) | `npm run test:e2e` | sama | Hanya bila task meminta dan environment mendukung |
| Semua repo | `git status --short` | tiap repo | Pisahkan perubahan task dari perubahan yang sudah ada sebelumnya |

Frontend memakai JavaScript/JSX, jadi tidak ada langkah typecheck.

Setiap hasil diklasifikasikan: `PASS`, `NEW ERROR`, `EXISTING / ENVIRONMENT ISSUE` (dengan bukti),
atau `NOT RUN` (dengan alasan).

---

## D. Smoke Test Runtime

Prasyarat:

- backend dan frontend dev berjalan;
- ada akun uji dengan hak akses menu yang tertaut `MstEmployee`;
- status versi instrumen di lingkungan uji diketahui.

Bila salah satu tidak tersedia, laporkan `MANUAL TEST: NOT FEASIBLE — <alasan>`. Task terkait
maksimal `🟡`, kecuali user mengecualikan butir ini lewat keputusan bertanggal.

Semua skenario memakai pasien samaran. Jangan mencatat identitas pasien atau kredensial nyata di
laporan.

| No | Skenario | Yang dicatat |
| --- | --- | --- |
| D1 | Buka menu untuk pasien samaran anak, dewasa, dan lansia → instrumen yang teresolusi sesuai usia | kode instrumen per pasien |
| D2 | Isi butir → skor dan band di UI sama dengan hasil server (`score/preview` atau respons simpan) | angka UI vs angka server |
| D3 | Simpan draf → muat ulang halaman → semua field kembali utuh, termasuk enum dan checklist | kode HTTP + field yang tidak kembali |
| D4 | Buka lagi → draf aktif terdeteksi, tidak membuat draf ganda | kode HTTP |
| D5 | Selesaikan → dokumen terkunci, kontrol nonaktif, TTD atau stempel verifikasi tampil | kode HTTP + tampilan |
| D6 | Klik ganda "Selesaikan & Kunci" atau ulangi request yang sama → hanya satu kejadian tercatat | jumlah jejak status |
| D7 | Koreksi/addendum (bila ada di alur) tercatat tanpa mengubah dokumen asli | kode HTTP + riwayat |
| D8 | Kolom ringkasan yang disinkronkan (mis. `HasFallRisk`, `FallRiskScore`) terisi benar | nilai dari `GET` |
| D9 | Tombol aksi yang tampil sama dengan `AvailableActions` dari server pada tiap status | daftar aksi |
| D10 | Negatif: akun tanpa profil pegawai → pesan 403 jelas di UI; field wajib kosong → pesan validasi; server gagal → tampilan error + coba lagi | kode HTTP + pesan |

---

## E. Tutup Pekerjaan

Butir di bawah **diperiksa** oleh skill ini. Yang menulis roadmap dan laporan adalah skill resmi
pemiliknya.

- [ ] Setiap task menu ini punya laporan di `<blueprint-root>/task/report/<lapisan>/<TASK-ID>.md`,
      ditulis `build-module-*`.
- [ ] Tanda task di roadmap aktif konsisten di semua titik: judul kartu, baris Status, grafik,
      tabel slice, dan register. `requirement-traceability` ikut diperbarui.
- [ ] Butir DoD yang dikecualikan tercatat sebagai keputusan user yang bertanggal.
- [ ] Bagian 1 dan 13 dokumen rencana kerja sudah diperbarui. Status dokumen diturunkan dari
      roadmap.
- [ ] Setiap usulan KK yang menahan task sudah punya ID register (`RWI-OQ-###`), atau disebut
      terbuka di serah terima.
- [ ] Daftar fallback (bagian 9 dokumen) sesuai dengan kode yang benar-benar ada.
- [ ] Checklist 8 butir `aturan-output-dokumentasi.md` lulus untuk dokumen rencana kerja.
- [ ] Tidak ada dokumen ganda untuk menu ini.
- [ ] Tidak ada data pasien, email akun, atau kredensial nyata di dokumen dan laporan.
- [ ] Tidak ada git, migration, eksekusi DB, atau perubahan konfigurasi yang dijalankan tanpa
      instruksi eksplisit.
- [ ] Tidak ada berkas di `QuilvianEngineeringSkills/**`, `~/.gemini/config/**`, atau `QuilvianV1/**`
      yang berubah.
