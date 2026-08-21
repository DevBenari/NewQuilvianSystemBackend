# Validation Matrix — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | `0.1.0` |
| Status | `draft` |
| Owner | Product/Domain Owner sementara sesuai `RWI-DEC-006` |
| `input_revision` | `00-interview-decisions.md` revision `2`; `02-backend-architecture.md` revision `0.1` |
| Dampak kompatibilitas | Seluruhnya baru, kecuali satu baris pada bagian 8 yang mengubah perilaku endpoint existing |

Pesan pada kolom "Pesan bagi pengguna" ditulis sebagaimana akan dibaca petugas di layar. Bukan
istilah teknis, bukan nama kolom.

---

## 1. Membuka admisi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Pasien wajib ada | `POST /episodes` | `PatientId` kosong atau tidak ditemukan | "Pasien belum dipilih." | 400 |
| Kunjungan wajib bertipe rawat inap | `POST /episodes` | Kunjungan yang dipilih bukan bertipe rawat inap | "Kunjungan yang dipilih bukan kunjungan rawat inap." | 422 |
| Satu kunjungan satu episode | `POST /episodes` | Kunjungan sudah punya episode | "Kunjungan ini sudah punya episode rawat inap." | 409 |
| DPJP wajib ditentukan | `POST /episodes` | `DoctorId` kosong | "Dokter penanggung jawab belum dipilih." | 400 |
| Unit layanan wajib bertipe rawat inap | `POST /episodes` | `ServiceUnitType` bukan `Inpatient` | "Unit layanan yang dipilih bukan unit rawat inap." | 422 |
| Kelas pasien wajib berlaku untuk rawat inap | `POST /episodes` | `IsForInpatient` bernilai salah | "Kelas perawatan yang dipilih tidak berlaku untuk rawat inap." | 422 |

## 2. Memesan tempat tidur

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Episode wajib `Draft` | `POST /bed-occupancies/reservations` | Episode bukan `Draft` | "Pemesanan tempat tidur hanya dapat dilakukan sebelum pasien ditempatkan." | 422 |
| Tempat tidur wajib aktif | idem | `MstBed.IsActive` salah | "Tempat tidur ini sedang tidak aktif." | 422 |
| Tempat tidur wajib dapat dipesan | idem | `IsReservable` salah | "Tempat tidur ini tidak dapat dipesan." | 422 |
| Tempat tidur tidak sedang ditutup | idem | `BedStatus` bernilai `Cleaning`, `Maintenance`, `Blocked`, atau `Inactive` | "Tempat tidur sedang tidak dapat dipakai. Keadaan saat ini: Perbaikan." | 422 |
| Tempat tidur belum dipesan orang lain | idem | Ada pemesanan aktif milik episode lain | "Tempat tidur ini sudah dipesan untuk pasien lain." | 409 |
| Tempat tidur belum ditempati | idem | Ada penempatan aktif | "Tempat tidur ini sedang ditempati pasien lain." | 409 |
| Satu episode satu pemesanan aktif | idem | Episode sudah punya pemesanan aktif | "Episode ini sudah memesan tempat tidur lain. Batalkan dulu pemesanan sebelumnya." | 409 |

## 3. Menempatkan pasien

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Episode wajib `Draft` | `POST /bed-occupancies/placements` | Episode bukan `Draft` | "Pasien sudah ditempatkan sebelumnya." | 409 |
| Seluruh aturan Kelayakan Penempatan | idem | Salah satu aturan pada bagian 2 tidak terpenuhi | Sama seperti pesan di bagian 2 | 409 atau 422 |
| Pemesanan yang gugur tidak menghalangi | idem | Pemesanan sudah `Expired` tetapi tempat tidur masih kosong | **Tidak ada penolakan.** Penempatan diteruskan tanpa peringatan | — |
| Isian admisi tetap utuh saat ditolak | idem | Penempatan ditolak karena tempat tidur diambil pasien lain | "Tempat tidur BD-RSMMC-00042 sudah ditempati pasien lain. Silakan pilih tempat tidur lain; isian admisi Anda tetap tersimpan." | 409 |

**Contoh berangka aturan baris ketiga.** Sdri. Wati memesan `BD-RSMMC-00042` pukul 09:15 untuk
Ny. Sari. Batasnya 2 jam, jadi gugur pukul 11:15. Ny. Sari baru sampai kamar pukul 11:40. Karena
tempat tidur itu masih kosong, penempatan **tetap berhasil** dan tidak ada peringatan apa pun.
Ini sesuai `RWI-RULE-015`.

## 4. Memindahkan pasien

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Episode wajib `Admitted` | `POST /bed-occupancies/placements/transfer` | Episode bukan `Admitted` | "Perpindahan hanya dapat dilakukan selama pasien masih dirawat." | 422 |
| Alasan medis wajib | idem | `TransferReason` kosong | "Alasan perpindahan wajib diisi." | 400 |
| Tempat tidur tujuan berbeda | idem | Tempat tidur tujuan sama dengan yang sekarang | "Tempat tidur tujuan sama dengan tempat tidur saat ini." | 400 |
| Seluruh aturan Kelayakan Penempatan | idem | Tidak terpenuhi | Sama seperti bagian 2 | 409 atau 422 |
| **Kewenangan per pasien** | idem | Pemohon adalah dokter, tetapi bukan DPJP aktif episode itu | "Hanya DPJP episode ini yang dapat memindahkan pasien. Alihkan tanggung jawab DPJP lebih dulu bila diperlukan." | 403 |
| Perpindahan utuh | idem | Salah satu langkah gagal di tengah jalan | "Perpindahan gagal. Pasien tetap berada di tempat tidur semula." | 500 |

**Catatan tentang baris kewenangan.** Aturan ini berlaku **hanya untuk pemohon berperan dokter**.
Kepala ruangan, perawat pelaksana, dan supervisor tetap boleh memindahkan tanpa menjadi DPJP,
sesuai `RWI-DEC-012` yang tidak dicabut.

## 5. Membatalkan admisi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Alasan wajib | `PATCH /episodes/{id}/cancel` | Alasan kosong | "Alasan pembatalan wajib diisi." | 400 |
| Wewenang saat `Draft` | idem | Bukan petugas admisi | "Anda tidak punya hak akses untuk tindakan ini." | 403 |
| Wewenang saat `Admitted` | idem | Bukan supervisor atau kepala ruangan | "Pembatalan setelah pasien dirawat hanya dapat dilakukan supervisor atau kepala ruangan." | 403 |
| Belum ada catatan klinis | idem | Sudah ada catatan klinis pada episode | "Episode ini sudah memiliki catatan klinis, sehingga tidak dapat dibatalkan." | 422 |
| Pelepasan tempat tidur menyatu | idem | Pelepasan tempat tidur gagal | "Pembatalan gagal. Tidak ada data yang berubah." | 500 |

**Catatan tentang baris keempat.** Selama slice dokumentasi klinis masih menunggu `DEC-INP-001`,
pemeriksaan "sudah ada catatan klinis" **belum dapat dijalankan sepenuhnya**. Pada MVP, pemeriksaan
memakai penanda pengganti yang tercatat pada `04-prd-to-mvp.md`, dan ini adalah pengurangan
kemampuan yang disadari, bukan kelalaian.

## 6. Keputusan pulang dan resume

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Hanya DPJP aktif | `POST /discharges/{id}/decide` | Pemohon bukan DPJP aktif | "Hanya DPJP episode ini yang dapat menyatakan pasien boleh pulang." | 403 |
| Cara pulang wajib dipilih | idem | `DischargeType` kosong atau `Unknown` | "Cara pulang wajib dipilih." | 400 |
| Cara pulang wajib termasuk yang berlaku | idem | Nilai di luar tiga yang berlaku | "Cara pulang yang dipilih belum tersedia pada versi ini." | 422 |
| Diagnosis utama wajib | `PATCH /discharges/{id}/summary/sign` | `PrimaryDiagnosisText` kosong | "Diagnosis utama wajib diisi sebelum resume ditandatangani." | 400 |
| Tujuan rujukan wajib bila dirujuk | idem | Cara pulang `Referred` dan `ReferralDestination` kosong | "Tujuan rujukan wajib diisi untuk pasien yang dirujuk." | 400 |
| Hanya DPJP aktif yang menandatangani | idem | Penandatangan bukan DPJP aktif | "Hanya DPJP episode ini yang dapat menandatangani resume." | 403 |
| Satu resume per episode | `PUT /discharges/{id}/summary` | Sudah ada resume | Resume yang ada diperbarui, bukan ditolak | — |

## 7. Penutupan episode

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Episode wajib `DischargePending` | `POST /discharges/{id}/close` | Status lain | "Episode hanya dapat ditutup setelah DPJP menyatakan pasien boleh pulang." | 422 |
| Resume wajib tertandatangani | idem | `SignedAt` kosong | "Resume pulang belum ditandatangani DPJP." | 422 |
| Butir wajib administrasi | idem | Ada butir `IsMandatory` yang belum ditandai | "Masih ada butir administrasi yang belum ditandai: Berkas administrasi pasien lengkap." | 422 |
| Kelayakan keuangan `Cleared` | idem | Nilai terakhir `Pending` atau `Blocked` | "Kelayakan keuangan belum dinyatakan lunas oleh kasir." | 422 |
| Wewenang menembus gerbang | `POST /discharges/{id}/close-with-override` | Bukan supervisor | "Hanya supervisor yang dapat menutup episode tanpa kelayakan keuangan." | 403 |
| Alasan wajib saat menembus | idem | Alasan kosong | "Alasan penutupan tanpa kelayakan keuangan wajib diisi." | 400 |
| Empat syarat lain tetap berlaku saat menembus | idem | Salah satu dari empat syarat lain belum terpenuhi | Sama seperti pesan masing-masing | 422 |

**Yang penting dipahami:** jalan keluar supervisor **hanya** menembus syarat kelayakan keuangan.
Keempat syarat lainnya tetap wajib, dan tidak ada satu pun peran yang dapat melewatinya.

## 8. Kelayakan keuangan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Wewenang | `POST /discharges/{id}/financial-clearance` | Bukan kasir atau billing | "Hanya petugas kasir atau billing yang dapat menandai kelayakan keuangan." | 403 |
| Catatan wajib | idem | `Note` kosong | "Catatan wajib diisi saat menandai kelayakan keuangan." | 400 |
| Episode belum ditutup | idem | Episode `Closed` tanpa sesi koreksi | "Episode sudah ditutup." | 409 |

## 9. Sesi koreksi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Hanya supervisor | `POST /episodes/{id}/correction-sessions` | Bukan supervisor | "Hanya supervisor yang dapat membuka kembali episode." | 403 |
| Episode wajib `Closed` | idem | Status lain | "Sesi koreksi hanya untuk episode yang sudah ditutup." | 422 |
| Alasan wajib | idem | Alasan kosong | "Alasan membuka kembali episode wajib diisi." | 400 |
| Satu sesi terbuka | idem | Sudah ada sesi terbuka | "Episode ini sedang dalam sesi koreksi yang belum ditutup." | 409 |
| Daftar perubahan wajib | `PATCH .../correction-sessions/{sessionId}/close` | `ChangedFieldSummary` kosong | "Tuliskan apa saja yang diubah sebelum menutup sesi koreksi." | 400 |

## 10. Perubahan aturan pada endpoint yang sudah ada

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Status penghunian bukan wewenang admin | `PATCH /health-services/master-data/beds/{id}/availability` | Nilai yang dikirim `Reserved` atau `Occupied` | "Status Terisi dan Dipesan hanya dapat diubah lewat modul Rawat Inap. Untuk menutup tempat tidur sementara, pakai status Pembersihan, Perbaikan, atau Diblokir." | 422 |

Ini satu-satunya baris pada dokumen ini yang **mengubah perilaku endpoint yang sudah dipakai**.
Dasarnya `RWI-RULE-027` aturan 4 dan 5. Persetujuan pemilik `MasterData` tercatat sebagai
`RWI-OQ-033` dan belum ada.

## 11. Aturan penanganan waktu

| Aturan | Penjelasan |
| --- | --- |
| Seluruh waktu disimpan sebagai UTC | Mengikuti konvensi project; seluruh model existing memakai `DateTime.UtcNow` |
| Kedaluwarsa pemesanan dihitung saat dibaca | `RWI-DEC-007`. Tidak ada program penjadwal |
| Episode `Draft` telantar dihitung saat dibaca | `RWI-DEC-030`. Tidak ada program penjadwal |
| Lama dirawat dihitung dari **selisih tanggal**, bukan selisih jam | `RWI-RULE-019`. Hasil paling sedikit 1 hari |
| Lama dirawat bertambah pada pergantian tanggal | Bukan setiap genap 24 jam |

**Contoh berangka lama dirawat.** Tn. Budi masuk 21 September pukul 22:30 dan pulang 22 September
pukul 06:00. Selisih jamnya hanya 7,5 jam, tetapi tanggalnya berbeda, sehingga lama dirawat
tercatat **1 hari**, bukan 0 hari.

---

## 12. Traceability

| Bagian | Requirement dan decision asal |
| --- | --- |
| 1 | `RWI-RULE-005`, `RWI-DEC-011` |
| 2, 3 | `RWI-RULE-001`, `RWI-RULE-002`, `RWI-RULE-015`, `RWI-RULE-027` |
| 4 | `RWI-RULE-006`, `RWI-RULE-008`, `RWI-RULE-016`, `RWI-RULE-030` |
| 5 | `RWI-RULE-004`, `RWI-DEC-010` |
| 6 | `RWI-RULE-011`, `RWI-RULE-032`, `RWI-DEC-045` |
| 7 | `RWI-RULE-009`, `RWI-RULE-010`, `RWI-RULE-018` |
| 8 | `RWI-RULE-028`, `RWI-DEC-040` |
| 9 | `RWI-RULE-020`, `RWI-DEC-028` |
| 10 | `RWI-RULE-027`, `RWI-DEC-039` |
| 11 | `RWI-RULE-002`, `RWI-RULE-019`, `RWI-RULE-022` |
