# Acceptance Test Matrix — Modul Radiologi

| Field | Value |
|---|---|
| Contract version | `RAD-TEST-001` |
| Revision | `2` |
| Status | `draft` |
| Backend SHA | `64da911` |
| Input | `RAD-ARCH-BE-001`, `RAD-STATE-001`, `RAD-VAL-001`, `RAD-PERM-001`, `RAD-ARCH-FE-001` |

Matriks ini memuat **jalur gagal**, bukan hanya jalur berhasil. Uji yang hanya membuktikan
keadaan normal tidak membuktikan apa pun tentang keselamatan.

---

## 1. Aturan Keselamatan — Slice `S4`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-12 | Aturan berstatus `Draft` tidak ikut dinilai gerbang keselamatan | Unit, tanpa database | Study dengan satu aturan `Draft` saja ditolak dengan pesan "aturan belum ditetapkan" |
| AC-12 | Aturan berstatus `PendingApproval` tidak ikut dinilai | Unit | Sama seperti di atas |
| AC-13 | Admin Radiologi mengesahkan aturannya sendiri | Integrasi | Ditolak `403` |
| AC-13 | Penanggung jawab klinis mengesahkan | Integrasi | Berhasil, status menjadi `Active` |
| AC-14 | Pengesahan menaikkan versi tepat satu kali | Integrasi | `RuleVersion` naik dari `1` ke `2`, bukan `3` |
| AC-15 | Menolak aturan tanpa mengisi alasan | Integrasi | Ditolak `400`, status tetap `PendingApproval` |
| AC-16 | Aturan berubah setelah study lolos | Integrasi | `SafetyRuleVersionAtClearance` pada study **tidak berubah** |
| AC-17 | Alat tanpa aturan aktif | Unit component | Layar menampilkan peringatan |
| — | Dua aturan aktif untuk kombinasi alat, pemeriksaan, dan butir yang sama | Integrasi | Pengesahan kedua ditolak `409` |
| — | Mengubah aturan yang sedang `Active` | Integrasi | Ditolak `403` |

---

## 2. Gerbang Keselamatan — Slice `S3`

Sebagian sudah diuji berkas `RadiologySafetyGateTests.cs` yang ada. Yang di bawah melengkapi.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| Fail-closed | Tidak ada satu pun aturan untuk sebuah alat | Unit | Acquisition ditolak, pesan menyebut aturan belum ditetapkan |
| Butir wajib | Butir wajib berkeadaan `Pending` | Unit | Ditolak, pesan menyebut **kode butir** yang menahan |
| Butir wajib | Butir wajib berkeadaan `Failed` | Unit | Ditolak, pesan menyebut butir dinyatakan tidak aman |
| Butir tidak wajib | Butir tidak wajib berkeadaan `Failed` | Unit | **Diloloskan** — perbedaan wajib dan tidak wajib harus terjaga |
| `NotApplicable` | Butir wajib berkeadaan `NotApplicable` | Unit | Diloloskan, tetapi jejaknya tetap dapat dibedakan dari `Passed` |
| Ketiadaan baris | Butir wajib tanpa baris jawaban sama sekali | Unit | Diperlakukan belum dijawab, bukan lolos |

---

## 3. Hasil Bacaan — Slice `S9`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-1 | Residen menulis draf lalu mengesahkan sendiri | Integrasi | Ditolak `403`, pesan "Draf yang Anda tulis harus disahkan dokter radiolog" |
| AC-2 | Radiolog menulis draf lalu mengesahkan sendiri | Integrasi | **Berhasil** |
| AC-2 | Radiografer menulis draf lalu mengesahkan sendiri | Integrasi | Ditolak `403` |
| AC-2 | Bantuan AI menulis draf, radiolog mengesahkan | Integrasi | Berhasil |
| AC-2 | Bantuan AI menulis draf, disahkan sistem | Integrasi | Ditolak `403` |
| AC-3 | Residen menulis draf, lalu perannya diubah menjadi radiolog, lalu ia mengesahkan draf lamanya | Integrasi | **Tetap ditolak `403`** — peran dibekukan pada draf |
| AC-4 | Radiolog menulis dan mengesahkan sendiri | Integrasi | Riwayat mencatat penulis dan pengesah sebagai dua baris berbeda dengan orang yang sama |
| — | Menulis bacaan atas study yang `IsUsable` bernilai `false` | Integrasi | Ditolak `422` |
| — | Menulis bacaan atas study yang `IsUsable` masih `null` | Integrasi | Ditolak `422` |
| — | Membuat bacaan kedua atas study yang sama | Integrasi | Ditolak `409` |
| — | Merilis bacaan yang belum disahkan | Integrasi | Ditolak `409` |
| — | Menyimpan draf tanpa mengisi kesimpulan | Integrasi | Ditolak `400` |
| — | Dua radiolog mengesahkan draf yang sama bersamaan | Integrasi | Satu berhasil, satu ditolak `409`. **Hanya satu** baris pengesahan tersimpan |
| — | Orang lain mengubah draf yang bukan miliknya | Integrasi | Ditolak `403` |

---

## 4. Koreksi Berversi — Slice `S10`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-18 | Amandemen dirilis | Integrasi | Versi 1 **isinya tidak berubah satu huruf pun**; statusnya menjadi `Superseded` |
| AC-18 | Amandemen dirilis | Integrasi | `CurrentVersionNumber` pada induk menjadi `2` |
| AC-18 | Amandemen dirilis | Integrasi | Versi 2 menunjuk versi 1 lewat `PreviousVersionId` |
| — | Menulis amandemen tanpa mengisi alasan | Integrasi | Ditolak `400` |
| — | Menulis amandemen atas bacaan yang belum pernah dirilis | Integrasi | Ditolak `409` |
| — | Mengubah isi versi berstatus `Released` | Integrasi | Ditolak `403` |
| — | Menghapus versi mana pun | Integrasi | Ditolak `403`; tidak ada endpoint hapus |
| — | Amandemen atas amandemen | Integrasi | Versi 3 dibuat, versi 2 menjadi `Superseded`, versi 1 tetap `Superseded` |
| — | Aturan pengesahan pada amandemen | Integrasi | Sama persis dengan draf pertama — residen tetap tidak boleh mengesahkan sendiri |

---

## 5. Penyajian ke Rekam Medis — Slice `S14`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-19 | Bacaan dikoreksi lalu rekam medis dibuka | Integrasi | Menampilkan **versi terbaru**, tanpa langkah penyalinan apa pun |
| AC-20 | Modul Radiologi tidak dapat dihubungi | Unit component | Layar menampilkan pesan gangguan, **bukan** daftar kosong |
| AC-20 | Pasien memang belum punya bacaan | Unit component | Layar menampilkan "Belum ada hasil bacaan", berbeda dari pesan gangguan |
| AC-18 | Tidak ada tabel di luar Radiologi yang menyimpan isi bacaan | Uji arsitektur | Pencarian tipe tidak menemukan nama bertema salinan hasil di namespace lain |
| AC-21 | Slot `PatientClinicalDocumentSource.Radiology` | Uji arsitektur | Tidak diisi oleh alur hasil bacaan internal |

Uji arsitektur baris ketiga melengkapi `RawatInapTidakMemilikiSatuPunTabelSalinanHasilPenunjang`
yang sudah ada.

---

## 6. Data Induk Alat — Slice `S13`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-5 | Menambah alat baru lewat antarmuka | Integrasi | Berhasil, tanpa menyentuh database langsung |
| AC-6 | Butir keselamatan berbeda antar alat | Integrasi | MRI mewajibkan implan logam; USG tidak |
| — | Menambah alat dengan kode yang sudah dipakai | Integrasi | Ditolak `409` |
| — | Menambah alat tanpa mengisi nama | Integrasi | Ditolak `400` |
| — | Menonaktifkan alat yang masih dipakai aturan aktif | Integrasi | Ditolak `409` |
| — | Menonaktifkan butir yang masih dipakai aturan aktif | Integrasi | Ditolak `409` |

---

## 7. Kelayakan Tagih — Slice `S8`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| — | Citra dinyatakan layak | Integrasi | Tepat **satu** fakta terbit; `BillingFactSubmitted` menjadi `true` |
| — | Citra dinyatakan tidak layak | Integrasi | **Tidak ada** fakta terbit |
| — | Penilaian mutu diulang pada study yang sama | Integrasi | Fakta **tidak** terbit dua kali |
| — | Pengiriman fakta gagal | Integrasi | Study tetap `QualityAccepted`; `BillingFactSubmitted` tetap `false`; dapat dikirim ulang |
| — | Hasil bacaan dirilis | Integrasi | **Tidak ada** fakta tagih terbit |
| — | Study pengulangan berhasil | Integrasi | Fakta terbit membawa keterangan pengulangan beserta sebabnya |

---

## 7b. Daftar Kerja dan Penanda Cito — Slice `S12`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| AC-36 | Membuka daftar kerja CT-Scan | Integrasi | Hanya pemeriksaan CT-Scan yang muncul; MRI dan USG tidak |
| AC-37 | Daftar kerja dijalankan | Uji arsitektur | Query hanya menyentuh `RadOrder` dan `RadStudy`; **tidak ada tabel daftar kerja** |
| AC-39 | Satu pesanan cito dan empat pesanan biasa yang lebih tua | Integrasi | Pesanan cito berada di **urutan pertama** |
| AC-39 | Dua pesanan cito | Integrasi | Keduanya di atas, diurutkan menurut waktu pesanan |
| AC-41 | Study lahir dari pesanan bercito | Integrasi | Penanda cito ikut terbaca pada baris study di daftar kerja |
| AC-42 | Pesanan ditandai cito | Integrasi | Tersimpan siapa yang menandai dan kapan |
| AC-40 | Daftar kerja ditampilkan | Unit component | Penanda cito terlihat tanpa membuka rincian |
| — | `GET /worklist` tanpa `modalityId` | Integrasi | Ditolak `400` — alat wajib dipilih |
| — | Pemanggil lama membuat pesanan tanpa mengirim `IsUrgent` | Integrasi | Berhasil; `IsUrgent` bernilai `false`. **Kompatibilitas mundur terjaga** |

---

## 8. Hak Akses — Menutup `RAD-CAP-025`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| — | Setiap endpoint radiologi | Uji kontrak | Memuat `[AccessPermission(...)]` dengan string persis seperti `RAD-PERM-001` |
| — | Tidak ada endpoint tanpa atribut hak akses | Uji kontrak | Daftar endpoint tanpa atribut kosong |
| — | `RadSafetyRule : Create` dan `Approve` dipegang peran berbeda | Uji kontrak | Tidak ada satu peran memegang keduanya |
| — | Pemeriksaan `AuthorRoleSnapshot` ada di service | Uji kontrak | Bukan hanya didokumentasikan |

Mengikuti pola `LaboratoryAuthorityTests.cs` dan `BloodBankRoleAccessContractTests.cs`.

---

## 9. Privasi dan Logging

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| — | Menulis draf bacaan | Uji logging | Log memuat `EntityId`, controller, action, status — **tidak** memuat `Findings`, `Impression`, `Recommendation` |
| — | Mengisi butir keselamatan bercatatan | Uji logging | Log tidak memuat isi catatan |
| — | Membatalkan pesanan beralasan | Uji logging | Log tidak memuat `ClosureReason` |
| — | Membuka daftar bacaan | Uji logging | `GET` tidak dicatat |
| — | Pesan kesalahan | Uji unit | Tidak memuat nama pasien maupun data medis |

---

## 10. Migration

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| — | Migration 1 dijalankan pada database berisi aturan yang berlaku | Integrasi | Aturan yang tadinya `IsActive = true` menjadi `RuleStatus = Active`; **pemeriksaan tetap dapat berjalan** |
| — | Migration 1 dijalankan | Integrasi | Aturan yang tadinya tidak aktif menjadi `Inactive` |
| — | Migration 1 dijalankan | Integrasi | Index unik parsial memakai `RuleStatus`, bukan `IsActive` |
| — | Migration 1 dimundurkan | Integrasi | Enam kolom hilang, filter index kembali; **tidak ada data hilang** |
| — | Migration 2 dijalankan | Integrasi | Dua tabel baru terbentuk; tabel lain tidak tersentuh |
| — | Migration 3 dijalankan pada database berisi pesanan lama | Integrasi | Seluruh pesanan lama bernilai `IsUrgent = false`; tidak ada yang mendadak terbaca cito |
| — | Migration 3 dimundurkan | Integrasi | Tiga kolom dan index-nya hilang; pesanan tetap utuh |

> **Uji migration paling penting adalah baris pertama.** Bila `RuleStatus` salah diisi, seluruh
> pemeriksaan langsung tertolak begitu migration dijalankan — karena gerbang bersifat
> fail-closed. Ini kegagalan yang berdampak langsung pada pelayanan pasien.

---

## 11. Frontend

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| Bagian 5 butir 1 | Penulis bukan-radiolog membuka drafnya | Unit component | Tombol Sahkan tersembunyi atau nonaktif |
| Bagian 5 butir 2 | Bacaan belum dirilis | Unit component | Tidak tampil di layar dokter pengirim |
| Bagian 5 butir 4 | Permintaan hasil gagal | Unit component | Pesan gangguan, bukan daftar kosong |
| Bagian 5 butir 5 | Alat tanpa aturan aktif | Unit component | Peringatan tampil |
| Bagian 5 butir 6 | Setelah halaman ditutup | Unit component | Tidak ada isi bacaan tersisa di penyimpanan browser |
| Bagian 9 | Tombol Simpan ditekan dua kali | Unit component | Hanya satu permintaan terkirim |

---

## 12. Yang Belum Dapat Diuji

| Slice | Alasan |
|---|---|
| `S5` Pelewatan gerbang darurat | Belum dirancang; `DEC-RAD-001` |
| `S11` Temuan kritis | Belum dirancang; `DEC-RAD-002` |
| Pemantauan keterlambatan pesanan cito | Ditunda `RAD-DEC-013`; `RAD-OPEN-009` |

Ketiganya **tidak** boleh dimasukkan ke rencana pengujian sebelum keputusannya turun.

`S12` daftar kerja **sudah dapat diuji** sejak Amendment pass 2026-09-09 — lihat bagian 7b.
