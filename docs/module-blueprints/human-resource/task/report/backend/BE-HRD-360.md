# Laporan Perubahan Backend — `BE-HRD-360`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-360` |
| Judul | Layanan daftar dan ringkasan kelengkapan data penggajian (`WorkforcePayrollDataService.ListAsync`, `SummaryAsync`) |
| Slice | `S-A8` / `MVP-69` — gelombang 1 |
| Roadmap | `roadmap/backend-roadmap.md` bagian Slice S-A8, kartu `BE-HRD-360` |
| Trace | `GET api/v1/corporate/human-resource/workforce-core/payroll-data` dan `GET .../payroll-data/summary`; Aturan A dan B; `HRD-DEC-219` (daftar tanpa nominal, nomor disamarkan), `HRD-DEC-222` (perawat dari profesi); `INT-HR-108`; Acceptance §35 `AT-HRD-WPD-*` |
| Contract version | `v27` bagian 33 — approved 16 November 2026 (`HRD-DEC-015`); tidak diubah |
| Dependency | `BE-HRD-003` ✅ [B1]; gerbang G1 tertutup |
| Klasifikasi | `SMALL` — skor 2: satu repository (0), sekitar 8 berkas diperiksa termasuk pembaca Payroll (1), 1 berkas source diubah (urutan action saja) (0), tanpa perubahan API/schema (0), data sensitif dibaca tetapi tidak dibuka di daftar (0), ditambah 1 untuk verifikasi provider-nyata |
| Task mode | `BACKEND` (instruksi pengguna 18 November 2026 "lanjutkna" atas tawaran mengerjakan `BE-HRD-360`) |
| Target tulis | `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforcePayrollDataController.cs` (urutan action); uji baru `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePayrollDataListTests.cs`; dokumen roadmap/traceability/indeks uji/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a17a2bc3` (branch `yoga`); perubahan `BE-HRD-353`–`358`, `361`, `362`, `FIX-002`–`004` belum di-commit |
| Tanggal | 18 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terbukti: uji baru 4/4, proyek uji HR 264/264, kontrol negatif enam mutasi tertangkap; lewat API devYoga (baca saja) populasi 45 orang sama dengan basis data, filter/pencarian/halaman sesuai kontrak, tanpa nominal dan tanpa nomor lengkap, status sama dengan Aturan B dan dengan Kelengkapan Data Payroll. Gerbang G3 ditutup pemilik 18 November 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `WorkforceCore` |
| Pemilik / prefix registry | `Human Resource` / `Wfp` (`WfpSalaryAssignment`, `WfpBankAccount`) dan master `Mst*` HRD (`MstEmployee`, `MstDoctor`, `MstProfession`); hanya baca, tanpa entity baru |
| Keberlakuan | `NEW CODE` (source slice S-A8, 16 November 2026); task ini memverifikasi dan mengubah urutan deklarasi action |
| QBE yang berlaku | `QBE-SVC-001` (akses data lewat service), `QBE-API-001` (route dan `ApiResponse`), `QBE-PERM-001` (`[AccessPermission]` = `ControllerName` + nama `[AccessAction]`), `QBE-DTO-001` (DTO sesuai kontrak) |
| Tidak berlaku | `QBE-DB-*` (tanpa schema/migration), `QBE-TXN-001`/`QBE-AUD-001`/`QBE-LOG-001` (baca saja, tanpa tulis dan tanpa log), `QBE-VAL-001` (query tanpa penolakan; nilai di luar batas dijepit) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Kondisi as-is terhadap kontrak

| Butir kontrak v27 §33 / aturan slice | Source (as-is) | Hasil |
| --- | --- | --- |
| `GET /` dan `GET /summary`, hak `WorkforcePayrollData : Read` | `List` dan `Summary` dengan `[AccessPermission("WorkforcePayrollData", "Read")]` | Sesuai |
| Query `search`, `personType`, `completeness`, `pageNumber` (min 1), `pageSize` (bawaan 25, dijepit 1–100) | `ListAsync`: perbandingan tanpa membedakan huruf besar, `Math.Max(1, …)`, `Math.Clamp(…, 1, 100)` | Sesuai |
| Aturan A: karyawan dan dokter aktif, termasuk yang belum punya data; karyawan sekaligus dokter dihitung sekali sebagai karyawan | `LoadPeopleAsync`: karyawan dulu, dokter hanya bila `WorkforceProfileId` belum terlihat | Sesuai |
| `HRD-DEC-222`: perawat = karyawan yang kode/nama/kelompok profesinya memuat "perawat"/"nurs"; bidan bukan perawat | `IsNurse`: pengecualian "bidan"/"midwi" diperiksa lebih dulu | Sesuai |
| `HRD-DEC-219`: daftar tanpa gaji pokok; nomor hanya empat karakter terakhir | `WorkforcePayrollDataItemResponse` tidak punya ruas nominal; `MaskAccountNumber` → `****` + empat terakhir (nomor ≤ 4 karakter → `****`) | Sesuai |
| Aturan B: penetapan = utama aktif dengan tanggal mulai terbaru; rekening = payroll aktif (terverifikasi → utama → terbaru); lengkap = disetujui **dan** terverifikasi | `LoadOverviewAsync` dengan urutan yang sama dengan rencana pembuatan profil payroll di `PayrollDataReadinessService` (baris 284–320) | Sesuai |
| Ringkasan: `totalPeople`, `withApprovedSalaryAssignment`, `withVerifiedBankAccount`, `complete` | `SummaryAsync` menghitung dari populasi yang sama dengan daftar | Sesuai |

Tidak ada delta perilaku terhadap kontrak.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforcePayrollDataController.cs` | Action `Summary` dipindahkan ke atas `List` (tepat sesudah konstruktor dan pembantu `CurrentUserId`), sehingga di Swagger `GET …/summary` tampil paling atas. Ini mengikuti preferensi pemilik produk. Isi action tidak berubah. CRLF dipertahankan (125 CR = 125 baris) |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePayrollDataListTests.cs` (baru) | 4 uji: populasi (nonaktif/terhapus keluar, karyawan sekaligus dokter sekali, ringkasan sama), jenis/filter/pencarian/halaman (profesi meniru master devYoga: "Perawat" dan "Bidan" berkelompok `Nursing`; 126 orang → halaman 100 + 26), tanpa nominal dan nomor disamarkan (refleksi DTO + serialisasi JSON), dan Aturan B dibandingkan langsung dengan `PayrollDataReadinessService` |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test --filter WorkforcePayrollDataListTests` | 4/4 lulus | `PASS` | Keluaran `dotnet test` |
| — Populasi | Sari, Rudi (karyawan sekaligus dokter → `Karyawan`), dr. Bima tampil; karyawan nonaktif/terhapus dan dokter nonaktif tidak; ringkasan 3/0/0/0 | `PASS` (AC1) | idem |
| — Jenis dan filter | "Perawat" → Perawat; "Bidan" (kelompok `Kebidanan` maupun `Nursing`) → Karyawan; filter jenis tanpa membedakan huruf besar; Lengkap/Belum lengkap; cari "  bUnGa  " dan "nip-001"; `pageSize` 500 → 100 (126 data, 2 halaman), `pageSize`/`pageNumber` 0 → 1 | `PASS` (AC2) | idem |
| — Kerahasiaan | Tidak ada ruas `decimal` pada DTO baris; JSON daftar tanpa `12345678`, tanpa nomor lengkap, tanpa nama pemilik; `****7890`, nomor "123" → `****` | `PASS` (AC3) | idem |
| — Aturan B | Andi (penetapan 2025 disetujui + 2026 belum) → menunggu persetujuan, rekening terverifikasi lama didahulukan dari yang baru; Dewi (hanya bukan-utama/nonaktif disetujui, non-payroll terverifikasi) → belum ada penetapan, rekening utama dipilih; ringkasan 4/1/2/1; status gaji dan rekening = `PayrollDataReadinessService` untuk orang dengan satu penetapan utama, populasi identik | `PASS` (AC4) | idem |
| Kontrol negatif (enam putaran; berkas dipulihkan dari salinan dan dibandingkan byte demi byte tiap putaran) | M1 de-duplikasi dokter dihapus → AC1 gagal; M2 pengecualian bidan dihapus → AC2 gagal; M3 batas 100 dihapus → AC2 gagal; M4 nomor tidak disamarkan → AC3 dan AC4 gagal; M5 penetapan terlama dipakai → AC4 gagal; M6 rekening terverifikasi tidak didahulukan → AC4 gagal | `PASS` | Keluaran `dotnet test` per mutasi |
| Proyek uji HR (`dotnet test -p:RunAnalyzers=False`, termasuk build penuh project aplikasi) | 264/264 lulus (260 + 4 baru); build 0 galat. Sesudah urutan action diubah: tiga kelas payroll-data 18/18 | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2457 identitas kanonik dari 2457 kemampuan terdaftar." | `PASS` | `be-360.log` |
| API devYoga (superadmin, **baca saja**) — populasi | Bawaan `pageSize` 25; total 45 = 28 karyawan aktif + 17 dokter aktif (0 tumpang tindih), himpunan id sama persis dengan basis data, tanpa ganda; 43 orang tanpa data penggajian ikut tampil | `PASS` (AC1) | `at-360.mjs`, `at-log.jsonl` ("360 …") |
| — Jenis, filter, cari, halaman | Karyawan 28, Perawat 0, Dokter 17 (`dokter` huruf kecil juga 17), jumlah = total; Lengkap = Dewi Anggraeni, Lengkap + Belum lengkap = total; cari nomor huruf kecil dan potongan nama huruf besar menemukan orangnya; `pageSize` 500 → 100; `pageSize`/`pageNumber` 0 → 1; urut nama | `PASS` (AC2) | idem |
| — Kerahasiaan | Tidak ada ruas gaji pokok; 0 nomor rekening lengkap (dari seluruh baris `WfpBankAccount`) dan 0 gaji pokok di JSON daftar; 2 baris bermasker, semuanya `****` + empat terakhir sesuai basis data | `PASS` (AC3) | idem |
| — Aturan B | 0 beda antara status di daftar dan status yang dihitung dari basis data menurut Aturan B; 0 beda dengan Kelengkapan Data Payroll (`GET …/payroll-data-readiness`, 45 orang); devYoga 0 orang dengan lebih dari satu penetapan utama aktif | `PASS` (AC4) | idem |
| — Ringkasan | 45 / 1 / 1 / 1, sama dengan hitungan dari daftar | `PASS` | idem |
| — Swagger dokumen `corporate` | Urutan: `GET …/summary`, `GET …/`, `GET …/{id}`, `PUT …/{id}`; tidak ada deskripsi endpoint | `PASS` | `at-log.jsonl` ("360 Swagger corporate") |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Seluruh karyawan dan dokter aktif tampil, termasuk yang belum punya data; karyawan sekaligus dokter sekali | Terpenuhi | Harness + devYoga (45 = basis data) |
| 2. Perawat terpisah; filter jenis, kelengkapan, pencarian bekerja; paling banyak 100 baris | Terpenuhi | Harness (perawat/bidan) + devYoga (filter, cari, batas halaman; Perawat 0 sesuai data) |
| 3. Tanpa gaji pokok; nomor hanya empat karakter terakhir | Terpenuhi | Harness + devYoga |
| 4. Penetapan dan rekening yang dinilai sama dengan yang dibaca Payroll (Aturan B) | Terpenuhi | Harness (dibandingkan dengan `PayrollDataReadinessService`) + devYoga (0 beda) |
| Gerbang G3 | **Ditutup** oleh pemilik teknis dan produk, 18 November 2026: aturan teks profesi sesuai master profesi devYoga (10 profesi; "Perawat" → Perawat, "Bidan" kelompok `Nursing` → Karyawan) | Pertanyaan dan jawaban sesi kerja 18 November 2026 |
| Migration | Tidak ada migration dan tidak ada data yang ditulis | — |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Catatan data devYoga | 27 dari 28 karyawan aktif belum diisi profesinya, sehingga semuanya tampil sebagai Karyawan dan filter Perawat kosong. Ini soal kelengkapan Master Karyawan, bukan aturannya (dasar penutupan G3) |
| Risiko tersisa (milik Payroll) | Ringkasan Kelengkapan Data Payroll menilai "ada penetapan utama yang disetujui", sedangkan daftar HRD (kontrak v27 Aturan B) menilai penetapan utama **terbaru**. Keduanya hanya berbeda bila satu orang punya lebih dari satu penetapan utama aktif (mis. 2025 disetujui, 2026 belum): Payroll menampilkan "disetujui", HRD "menunggu persetujuan". devYoga saat ini 0 kasus. Simpan lewat slice ini selalu mengubah penetapan terbaru, sehingga kasus itu hanya muncul dari endpoint penetapan gaji HRD yang lama. Tidak diubah di task ini (kode Payroll) |
| Untuk pass blueprint | Penutupan G3 dicatat di tabel gerbang roadmap; keputusan belum ditambahkan ke `00-interview-decisions.md` (artefak blueprint terkunci hash) |
| Perubahan sampingan | `NONE`. Mutasi kontrol negatif dipulihkan identik |
| Interupsi | `NONE` |
| Status Git | 1 berkas source diubah (urutan action), 1 berkas uji baru, laporan ini (baru), dokumen roadmap/traceability/indeks uji. Tidak ada commit |
| Task berikutnya | `BE-HRD-363` — verifikasi slice S-A8 (kontrak v27, hak akses di Akses Role, build, QBE). Termasuk catatan `BE-HRD-362`: action `Save` belum mendeklarasikan `[ProducesResponseType]` `403` (Swagger memastikan keempat endpoint tanpa respons `403`) |
