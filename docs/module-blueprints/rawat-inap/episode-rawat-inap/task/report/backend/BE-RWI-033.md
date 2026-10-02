# Laporan Perubahan Backend — `BE-RWI-033`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-033` |
| Judul | Bukti penerimaan lengkap dan traceability tertutup |
| Slice | Penutup modul; kesiapan masuk `/qv-verify` |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-033` |
| Trace | `RWI-AC-001` s.d. `RWI-AC-149`; `testing/acceptance-test-matrix.md`; `roadmap/requirement-traceability.md` |
| Contract version | API `0.4.0` |
| Dependency | Seluruh task `BE-RWI-001` s.d. `BE-RWI-032` — **semuanya selesai** per 1 September 2026 |
| Klasifikasi | `MEDIUM`, skor 7: repository 0, berkas diperiksa 3, berkas diubah 2, logika bisnis 0, kontrak API 2, database 0, keamanan/auth 0, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Dokumen tracked modul Rawat Inap saja. **Tidak ada source aplikasi yang disentuh** |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `514b1d8232720eb450bc40f6deea6c6661160c8d` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | **Selesai.** Keempat acceptance criteria terbukti. Modul siap dinilai `/qv-verify` |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Bounded context | `HealthServices / InPatientManagement` |
| Applicability | Dokumentasi; tidak ada `NEW CODE` maupun `TOUCHED LEGACY` pada source |
| QBE berlaku | `QBE-MOD-001` |
| Database authority | `NONE` |

---

## 1. Masalah yang diperbaiki

Roadmap menulis peringatan yang ternyata tepat: *"Task ini sering diperlakukan sebagai
formalitas dan dikerjakan asal lengkap. Ia justru satu-satunya tempat lubang cakupan ketahuan
sebelum modul dipakai pasien sungguhan."*

Pemeriksaan silang menemukan **enam puluh tujuh acceptance criteria** yang tidak muncul di
`acceptance-test-matrix.md` maupun `requirement-traceability.md` — tidak ditolak, tidak
dijelaskan, hanya tidak pernah disebut. Ditemukan juga **empat skenario UAT** tanpa pasangan,
dan **satu baris api contract** yang masih berstatus `Rencana` padahal perubahannya sudah
dikerjakan hari itu juga.

**Kenapa ini berbahaya.** Acceptance criteria yang tidak pernah ditunjuk terlihat sama persis
dengan acceptance criteria yang gagal: keduanya tidak punya bukti. Ketika modul dinilai siap
pakai, tidak ada yang dapat membedakan "sudah teruji tetapi belum dicatat" dari "memang belum
pernah diuji". Contoh nyata: `RWI-AC-030` — *tidak tersedia kolom keterangan apa pun yang
memungkinkan dokter bukan DPJP melewati penolakan perpindahan* — sebenarnya sudah dijaga
`InpBedTransferTests`, tetapi tidak ada satu dokumen pun yang mengatakannya. Sebaliknya
`RWI-AC-032` tentang visite dokter memang belum dapat diuji, dan itu benar. Keduanya tampak
identik sebelum task ini.

---

## 2. Yang dikerjakan

Task ini **memeriksa** kelengkapan, bukan menulis ulang matriks. Tidak ada satu test pun yang
ditambahkan, dan tidak ada satu baris source pun yang disentuh.

### 2.1 Berkas yang diperiksa

| Berkas | Alasan diperiksa |
| --- | --- |
| `00-interview-decisions.md` | Sumber definisi seluruh acceptance criteria bernomor |
| `04-prd-to-mvp.md` | Sumber definisi ke-33 skenario UAT |
| `testing/acceptance-test-matrix.md` | Menetapkan acceptance criteria mana yang sudah punya baris |
| `contracts/api-contract.md` | Menghitung status ke-51 baris endpoint |
| `QuilvianSystemBackend.Tests/InPatientManagement/` | Membuktikan kelas test yang ditunjuk benar-benar ada |
| Seluruh berkas `.md` modul | Mencari butir traceability yang berbunyi "menyusul" |

### 2.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `roadmap/requirement-traceability.md` | Bagian baru **Penutupan bukti penerimaan — `BE-RWI-033`**: daftar 67 acceptance criteria beserta penunjuk atau alasannya, pemasangan empat UAT yang tersisa, rekap status endpoint, dan daftar yang tetap terbuka |
| `contracts/api-contract.md` | Baris `PATCH /beds/{id}/availability` naik dari `Rencana perubahan perilaku` menjadi `Diterapkan`; catatan pemutakhiran 1 September 2026 |

### 2.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Hanya kolom **status**. Tidak ada endpoint, payload, maupun hak akses yang berubah |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 3. Hasil pemeriksaan

### 3.1 Kriteria 1 — endpoint api contract

| Butir | Hasil |
| --- | ---: |
| Baris endpoint | 51 |
| `Tersedia` | 50 |
| `Diterapkan` (baris perubahan perilaku, dinilai terpisah) | 1 |
| `Rencana` | **0** |

Baris ke-50 adalah `GET /discharges/{episodeId}/financial-clearance` yang dibuka `BE-RWI-034`,
sehingga jumlah endpoint baru modul kini 50, bukan 49. Baris ke-51 adalah
`PATCH /beds/{id}/availability`, yang memang dinilai terpisah karena ia perubahan perilaku pada
endpoint milik modul lain, bukan endpoint baru. ✅

### 3.2 Kriteria 2 — acceptance criteria

Decision log memuat 146 acceptance criteria bernomor.

| Keadaan | Jumlah |
| --- | ---: |
| Punya baris tersendiri pada test matrix | 79 |
| Dipetakan pada bagian penutup traceability | 67 |
| — terbukti oleh test yang benar-benar ada | 40 |
| — sebagian terbukti, sisanya tertulis alasannya | 4 |
| — di luar scope MVP dengan decision ID-nya | 23 |
| **Tanpa penunjuk maupun alasan** | **0** ✅ |

Empat yang "sebagian" beserta alasannya:

| ID | Yang belum terbukti | Alasan |
| --- | --- | --- |
| `RWI-AC-008` | Batas "belum ada catatan klinis" saat pembatalan admisi | Keenam jenis catatan milik `ClinicalManagement` dan `PharmacyManagement`; jalur bacanya tidak ada pada integration contract |
| `RWI-AC-020` s.d. `RWI-AC-022` | Dua dari lima cara pulang — meninggal dan kabur | Aturan klinisnya belum disahkan; `RWI-RULE-037` masih **BELUM FINAL**, `DEC-INP-007`, `RWI-OQ-039` |

Dua puluh tiga yang di luar scope seluruhnya bergantung pada tiga slice yang memang tidak masuk
MVP: dokumentasi klinis rawat inap (`DEC-INP-001`, mencakup visite dokter `RWI-RULE-017`),
serah terima IGD (`DEC-INP-002`, jalur `INP-S09`), dan integrasi obat pulang ke Farmasi
(`RWI-DEC-046`). Ketiadaannya adalah keadaan yang disengaja, bukan cakupan yang terlupa. ✅

### 3.3 Kriteria 3 — skenario UAT

Dua puluh sembilan dari 33 sudah berpasangan sebelum task ini. Empat sisanya — `UAT-02`,
`UAT-03`, `UAT-04`, dan `UAT-23` — dipasangkan pada bagian penutup traceability. Seluruh 33
skenario kini berpasangan. ✅

`UAT-02` diberi catatan: pasangannya membuktikan penolakan pada tingkat service, sedangkan
pertahanan sebenarnya adalah unique index parsial ditambah penguncian baris, yang hanya dapat
dibuktikan terhadap PostgreSQL.

### 3.4 Kriteria 4 — butir yang berbunyi "menyusul"

Seluruh berkas `.md` modul diperiksa. Sembilan belas kemunculan kata "menyusul" ditemukan, dan
**tidak satu pun** merupakan butir traceability yang menunda buktinya — seluruhnya kalimat
biasa, misalnya *"penugasan perawat sering menyusul beberapa menit setelah pasien tiba"* pada
`RWI-DEC-032`. ✅

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | Berhasil | `PASS` | `Build succeeded. 206 Warning(s), 0 Error(s)` |
| Seluruh project test `QuilvianSystemBackend.Tests` | 879 lulus dari 879 | `PASS` | `Failed: 0, Passed: 879, Skipped: 0` |
| Kriteria 1 — tidak ada baris endpoint berstatus `Rencana` | 0 dari 51 | `PASS` | Hitung ulang atas `contracts/api-contract.md` |
| Kriteria 2 — tidak ada acceptance criteria tanpa penunjuk maupun alasan | 0 dari 146 | `PASS` | Bagian penutup `requirement-traceability.md` |
| Kriteria 3 — seluruh skenario UAT berpasangan | 33 dari 33 | `PASS` | Bagian penutup `requirement-traceability.md` |
| Kriteria 4 — tidak ada butir traceability berbunyi "menyusul" | 0 | `PASS` | Pemeriksaan seluruh berkas `.md` modul |
| Kelas test yang ditunjuk benar-benar ada | 31 kelas terverifikasi | `PASS` | `QuilvianSystemBackend.Tests/InPatientManagement/` |

Uji manual: `NOT APPLICABLE` — task ini pemeriksaan dokumen terhadap source dan test yang ada.

**Tidak dijalankan:** project `QuilvianSystemBackend.BillingTests` — menuntut
`QUILVIAN_BILLING_TEST_DB` dan di luar scope.

---

## 5. Yang tetap terbuka

Modul siap dinilai `/qv-verify`. Yang tersisa bukan lubang traceability, melainkan bukti yang
menuntut lingkungan berjalan atau keputusan yang belum turun.

| Butir | Pemilik |
| --- | --- |
| Pembuktian **403** dari aplikasi berjalan memakai akun non-SuperAdmin (`BE-RWI-009`, `BE-RWI-014`) | Backend/API bersama QA |
| Test tabrakan dua transaksi terhadap **PostgreSQL** (`BE-RWI-011`, `UAT-02`) | Backend/API |
| Verifikasi daftar pantau dari layar kepala ruangan (`BE-RWI-018`) | Frontend bersama QA |
| Dua cara pulang yang aturan klinisnya belum disahkan (`RWI-OQ-039`, `RWI-DEC-059` masih `draft`) | Product/Domain bersama Clinical governance |
| Delapan butir hak akses baru wajib diberikan admin (`BE-RWI-034` bagian 6) | Admin sistem |
| `RWI-RISK-002` turun tetapi belum tertutup | Backend/API |
| Batas "belum ada catatan klinis" pada pembatalan admisi (`RWI-AC-008`) | Backend/API bersama pemilik `ClinicalManagement` |

---

## 6. Task berikutnya

Tidak ada task backend `BE-RWI-*` yang tersisa. Seluruh 36 task selesai. Langkah berikutnya
adalah penilaian kesiapan modul lewat `/qv-verify`, dengan daftar bagian 5 sebagai masukannya.
