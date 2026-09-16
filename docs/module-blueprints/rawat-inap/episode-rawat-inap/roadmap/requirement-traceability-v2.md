# Requirement Traceability V2 — Sub-modul Episode Rawat Inap

> Berkas ini **baru** dan hanya melacak amandemen terbatas revision `7`
> (`PRD-RWI-V2-001`). Traceability task lama `BE-RWI-001` s.d. `BE-RWI-078` dan
> `FE-RWI-001` s.d. `FE-RWI-062` tetap di [`requirement-traceability.md`](./requirement-traceability.md).

## Metadata

```yaml
blueprint_id: RWI-BP-001
blueprint_revision: 7
submodule: episode-rawat-inap
traceability_revision: 4
contract_version: 0.9.0
approval_decision: RWI-DEC-150
gate_closure_decision: RWI-DEC-151   # {GATE-YOGA} tertutup 2026-09-16
approved_by: "Muhammad Hamzah"
approved_at: "2026-09-16"
upstream_input: "PRD-RWI-V2-001 v2.0"
input_revision_hash: sha256:2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f
backend_source_sha: df3679c0d5b2f08106702153eb242d3a6cb2929b
frontend_source_sha: 1ce219b40f8e411f3c4e66975626ab33ae81616a
backend_roadmap: roadmap/backend-roadmap-v2.md
frontend_roadmap: roadmap/frontend-roadmap-v2.md
```

---

## 1. Requirement → desain → kontrak → task → bukti

| FR | Epic | Keputusan | Desain | Kontrak `0.9.0` | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FR-RI-191` | `RI-38` | `RWI-DEC-117` | `02-backend` 11 | API 10.1 | `BE-RWI-081` | `FE-RWI-067` [FE-DOK] | Acceptance 18.1; `UAT-45` | ✅ Terbukti di source. Census `assignedToMe` menyaring dari penugasan aktif dokter login. [BE-RWI-081](../task/report/backend/BE-RWI-081.md) |
| `FR-RI-192` | `RI-38` | `VAL-INP-11` | `02-backend` 11 | API 10.1 | `BE-RWI-081` | `FE-RWI-067` [FE-DOK] | Acceptance 18.1 | ✅ Terbukti di source. Akun tanpa data dokter menerima `200` berdaftar kosong beserta `emptyReason`, bukan `403`. [BE-RWI-081](../task/report/backend/BE-RWI-081.md) |
| `FR-RI-193` | `RI-39` | `RWI-DEC-099` | `02-backend` 11.8 E1 | API 10.2 | `BE-RWI-079`, `BE-RWI-080` | `FE-RWI-063` | Acceptance 18.2; `UAT-46` | ✅ Terbukti di source. Kolom `AssignmentPurpose` beserta check constraint, dan jalur tulis penugasan pendukung. Dialog UI penugasan pendukung terpasang pada panel detail episode. [BE-RWI-079](../task/report/backend/BE-RWI-079.md), [BE-RWI-080](../task/report/backend/BE-RWI-080.md), [FE-RWI-063](../task/report/frontend/FE-RWI-063.md) |
| `FR-RI-194` | `RI-39` | `RWI-DEC-130`; `INV-INP-12` | `02-backend` 11.8 E1 | `VAL-INP-01`, `08` | `BE-RWI-079`, `BE-RWI-080` | `FE-RWI-063` | Acceptance 18.2; `UAT-46`, `UAT-47` | ✅ Terbukti di source. `INV-INP-12` ditegakkan di service **dan** database; di UI peran terkunci ke Dokter Jaga saat LateDocumentation dan tombol simpan nonaktif bila waktu selesai kosong. [BE-RWI-079](../task/report/backend/BE-RWI-079.md), [BE-RWI-080](../task/report/backend/BE-RWI-080.md), [FE-RWI-063](../task/report/frontend/FE-RWI-063.md) |
| `FR-RI-195` | `RI-39` | `VAL-INP-09` | `02-backend` 11 | API 10.2 | `BE-RWI-080` | `FE-RWI-063` | Acceptance 18.2; `UAT-48` | ✅ Terbukti di source. Pengakhiran konsulen/dokter jaga lewat `PATCH .../{assignmentId}/end`; penugasan DPJP ditolak `409` `VAL-INP-09` dan di UI baris DPJP tidak memiliki tombol Akhiri; akses tombol dibatasi untuk kepala ruangan/supervisor. [BE-RWI-080](../task/report/backend/BE-RWI-080.md), [FE-RWI-063](../task/report/frontend/FE-RWI-063.md) |
| `FR-RI-196` | `RI-40` | `RWI-DEC-112` | `data-dictionary` 18.2–18.3 | data 18.2–18.3 | `BE-RWI-085` | `FE-RWI-064` | Acceptance 18.3; `UAT-49` | ✅ Terbukti di source. Enam kolom pada dua tabel; ketiga isian melewati baca, simpan, dan salinan versi. [BE-RWI-085](../task/report/backend/BE-RWI-085.md) |
| `FR-RI-197` | `RI-40` | `NFR-027` | `02-backend` 11 | API 10.3 | `BE-RWI-086` | `FE-RWI-064` | Acceptance 18.3; `UAT-49` | 🟡 Sebagian. Usulan berlabel sumber, tidak menyimpan, dan tahan sumber gagal sudah ada; **pengukuran waktu nyata `NFR-027` belum ada**. Sumber laboratorium belum tersedia. [BE-RWI-086](../task/report/backend/BE-RWI-086.md) |
| `FR-RI-198` | `RI-41` | `RWI-DEC-138`; `RM-DEC-003` | `02-backend` 11.8 E3 langkah 4 | `INT-INP-08` | `BE-RWI-082` | `FE-RWI-065` | Acceptance 18.4; `UAT-50` | ✅ Terbukti di source. Langkah 4 penutupan mengunci konsep lewat service pemiliknya, di dalam transaksi yang sama. [BE-RWI-082](../task/report/backend/BE-RWI-082.md) |
| `FR-RI-199` | `RI-41` | `RWI-DEC-143` | `02-backend` 11.8 E3 langkah 5 | `INT-INP-09`; API 10.4 | `BE-RWI-083` | `FE-RWI-065`, `FE-RWI-066` | Acceptance 18.4; `UAT-50` | 🟡 Sebagian. Daftar pantau pesanan tertagih sudah ada; **langkah 5 pembatalan belum dipasang** karena `PatientProcedureOrderService` (`BE-RWI-097`) belum ada. [BE-RWI-083](../task/report/backend/BE-RWI-083.md) |
| `FR-RI-200` | `RI-41` | `RWI-DEC-143`; `INT-KEP-15` | `02-backend` 11.8 E3 langkah 6 | `INT-INP-10` | `BE-RWI-087` | `FE-RWI-065` | Acceptance 18.4; `UAT-50` | ⛔ Terblokir `BE-RWI-114` [BE-KEP]. Tabel MAR belum ada; nol berkas source diubah. [BE-RWI-087](../task/report/backend/BE-RWI-087.md) |
| `FR-RI-201` | `RI-41` | `VAL-INP-13` s.d. `17` | `03-frontend` 12 | validation matrix | `BE-RWI-084` | `FE-RWI-065` | Acceptance 18.4; `UAT-51` | 🟡 Sebagian. Peringatan tidak menahan dan rollback utuh sudah terbukti di source; tiga dari empat peringatan dan dua dari empat angka akibat masih menunggu `BE-RWI-097` dan `BE-RWI-114`. [BE-RWI-084](../task/report/backend/BE-RWI-084.md) |

**Sebelas FR, sebelas baris, nol FR tanpa task, nol FR tanpa bukti acceptance.**

---

## 2. Kebutuhan non-fungsional

| NFR | Bunyi | Task | Cara membuktikan | Status |
| --- | --- | --- | --- | --- |
| `NFR-025` | Penutupan beserta akibatnya atomik pada PostgreSQL sungguhan | `BE-RWI-082`, `BE-RWI-083`, `BE-RWI-084`, `BE-RWI-087` | Galat buatan tiap langkah → nol perubahan, dijalankan pada container Postgres sekali pakai | 🟡 `NOT RUN`. Langkah yang terpasang berada di dalam transaksi penutupan dan blok `catch` melakukan rollback — terbukti dari pembacaan source, bukan dari percobaan. **Uji galat buatan belum dijalankan** karena menuntut aplikasi berjalan beserta data klinis; dikecualikan atas keputusan pemilik pekerjaan 16 September 2026 |
| `NFR-026` | Census `assignedToMe` memakai index penugasan per dokter; waktu aktif dibaca saat query tanpa proses latar | `BE-RWI-081` | Rencana eksekusi memakai index; uji batas 06.59 dan 07.01 | 🟡 `NOT RUN`. Index `IX_InpDoctorAssignment_DoctorId_Active` **terbukti lahir di database** (dibaca dari `pg_indexes` pada container Postgres 16) dan penyaringnya menyaring persis pada `DoctorId` lalu menilai `EndDateTime`. Yang belum ada adalah **rencana eksekusi query pada tabel berisi data nyata**: pada tabel kosong PostgreSQL memilih `Seq Scan` apa pun index-nya, sehingga `EXPLAIN` di sana tidak membuktikan apa pun. Uji batas 06.59/07.01 juga belum diambil |
| `NFR-027` | Usulan isian resume selesai paling lama 5 detik per sumber — **angka usulan desain** | `BE-RWI-086` | Pengukuran dicatat pada laporan task; angka nyata dilaporkan apa adanya | 🟡 `NOT RUN`. Pengukuran terpasang, lolos kompilasi, dan keluar pada `sourceTimings` setiap balasan, tetapi **angka dari data nyata belum ada** karena menuntut lingkungan berisi diagnosis, tindakan, resep pulang, dan bacaan radiologi. Bila pengukuran nanti jauh melampaui 5 detik, angkanya dilaporkan apa adanya dan dibawa kembali ke pemilik — bukan diubah di roadmap |

---

## 3. Skenario UAT dan task yang menanggungnya

| UAT | Epic | Jalur | Task BE | Task FE |
| --- | --- | --- | --- | --- |
| `UAT-45` | `RI-38` | Berhasil | `BE-RWI-081` | `FE-RWI-067` [FE-DOK] |
| `UAT-46` | `RI-39` | Berhasil | `BE-RWI-079`, `BE-RWI-080` | `FE-RWI-063` |
| `UAT-47` | `RI-39` | Gagal | `BE-RWI-080` | `FE-RWI-063` |
| `UAT-48` | `RI-39` | Gagal | `BE-RWI-080` | `FE-RWI-063` |
| `UAT-49` | `RI-40` | Berhasil | `BE-RWI-085`, `BE-RWI-086` | `FE-RWI-064` |
| `UAT-50` | `RI-41` | Berhasil | `BE-RWI-082`, `083`, `084`, `087` | `FE-RWI-065` |
| `UAT-51` | `RI-41` | Gagal | `BE-RWI-084` | `FE-RWI-065` |

---

## 4. Dependency yang keluar dari sub-modul ini

| Task di sini | Menunggu | Milik | Kenapa |
| --- | --- | --- | --- |
| `BE-RWI-082` ✅ | `BE-RWI-091` | `dokter-rawat-inap` | **Tidak lagi menahan rilis.** Mesin penguncian sudah ada dan langkah 4 terpasang; `BE-RWI-091` menambah **cakupan dokumen** yang punya registrasi `Draft`, tanpa perubahan source di sini
| `BE-RWI-083` 🟡 | `BE-RWI-097` | `dokter-rawat-inap` | **Masih menahan.** `PatientProcedureOrderService` belum ada di repository; menulis `TrxPatientProcedure` langsung dari `InPatientManagement` ditolak `02-backend-architecture.md` 11.3
| `BE-RWI-087` ⛔ | `BE-RWI-114` | `keperawatan` | **Masih menahan.** Tabel MAR belum ada sama sekali; `02-backend-architecture.md` 11.8: "Selama tabel dosis belum ada, langkah 6 tidak dipasang"

## 5. Dependency yang masuk ke sub-modul ini

| Task di luar | Menunggu | Kenapa |
| --- | --- | --- |
| `BE-RWI-091` s.d. `BE-RWI-098` (`dokter-rawat-inap`) | `BE-RWI-079` | Penjaga penulis klinis membaca tujuan penugasan — `02-module-map.md` 3.4.1 gelombang V1 |
| `FE-RWI-067` (`dokter-rawat-inap`) | `BE-RWI-081` | Daftar pasien ruang kerja dokter memakai census `assignedToMe` |
| `FE-RWI-074` (`dokter-rawat-inap`) | `BE-RWI-085`, `BE-RWI-086` | Tab Resume Medis memakai kontrak resume yang sama |

---

## 6. Gerbang yang belum tertutup

| Gerbang | Jenis | Menahan apa | Pemilik |
| --- | --- | --- | --- |
| ~~Pemberitahuan `INT-INP-08`~~ | **TERTUTUP 2026-09-16** `RWI-DEC-151` | ~~DoD `BE-RWI-082`~~ — cukup merujuk keputusan itu | Yoga Aji Pratama ✅ |
| Nasib pesanan tertagih — 22.7 nomor 1 | Keputusan produk | Tidak menahan; menilai ulang cakupan `BE-RWI-083` dan `FE-RWI-066` | Muhammad Hamzah + pemilik Billing |
| Isi minimal resume sebelum tanda tangan — 22.7 nomor 3 | Gerbang produksi | Pemakaian pada pasien sungguhan | Pemilik klinis, **belum ditunjuk** |
| ~~Persetujuan Yoga Aji Pratama atas pemanggil penguncian — 22.7 nomor 4~~ | **TERTUTUP 2026-09-16** `RWI-DEC-151` | ~~Rilis `RI-V2-1`~~ — bebas | Yoga Aji Pratama ✅ |

---

## 7. Catatan revision

| Revision | Tanggal | Isi |
| ---: | --- | --- |
| `1` | 2026-09-16 | Dibuat `plan-module-delivery` fase `RLN-PH-07` sesudah `RWI-DEC-150`. Sebelas FR amandemen terbatas revision `7` dipetakan ke sembilan task backend dan empat task frontend. Ditulis sebagai berkas terpisah atas permintaan pemilik agar berkas lama tetap terbaca |
| `2` | 2026-09-16 | Bukti gelombang eksekusi pertama dimasukkan. Empat FR `✅`, tiga FR `🟡`, satu FR `⛔`. Ketiga NFR berstatus `NOT RUN`: butir verifikasi `dotnet build`, verifikasi skema, uji migration, uji galat buatan, rencana eksekusi query, dan pengukuran waktu sumber **dikecualikan atas keputusan pemilik pekerjaan 16 September 2026** yang menyatakan akan menjalankan build sendiri sesudah implementasi source selesai. Pengecualian itu tidak menjangkau acceptance criteria yang belum ada source-nya — `FR-RI-199`, `FR-RI-200`, dan `FR-RI-201` tetap belum penuh karena slice modul lain belum mendarat, bukan karena bukti yang belum diambil |
| `3` | 2026-09-16 | **Verifikasi benar-benar dijalankan.** `dotnet build` pada project aplikasi: `0 Error(s)`, `211 Warning(s)`, `00:04:31` — nol `error CS`. `dotnet ef migrations has-pending-model-changes`: `No changes have been made to the model since the last migration`, membuktikan suntingan tangan pada model snapshot cocok dengan model. Migration `E1` dan `E2` dijalankan maju dan mundur pada container Postgres 16 sekali pakai beserta pembacaan katalog; kedua penjaga rollback terbukti menolak dengan pesan dan jumlah baris yang benar; check constraint `INV-INP-12` diuji 6 kasus. Container dibuang, dan **tidak satu pun migration diterapkan ke database dev, staging, atau production.** Yang tetap `NOT RUN`: `UAT-46` s.d. `UAT-51`, rencana eksekusi query `NFR-026`, uji galat buatan `NFR-025`, dan pengukuran waktu `NFR-027` — seluruhnya menuntut aplikasi berjalan beserta data klinis |
| `4` | 2026-09-16 | `FE-RWI-063` selesai diimplementasikan pada frontend (`QuilvianSystemFrontendDev`). Komposisi dialog dokter pendukung `FE-INP-21`, aksi tambah dan akhiri penugasan, penguncian peran dan waktu selesai pada `LateDocumentation`, penyembunyian tombol bagi selain kepala ruangan/supervisor (`UAT-48`), penanganan galat 422, dan auto-refresh riwayat penugasan terbukti di source. `npm run lint:errors` bersih (0 error), `npm run build` sukses (exit code 0). Laporan tracked: [FE-RWI-063.md](../task/report/frontend/FE-RWI-063.md) |
