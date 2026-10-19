# Penilaian Putaran B6 dan Keselarasan Perbaikan dengan Blueprint Rawat Inap

| Field | Nilai |
| --- | --- |
| Tanggal | 18 Oktober 2026 |
| Penulis | Agent pengembang IGD, atas permintaan Rizki Gunawan (Product/Domain Owner IGD) |
| Sifat | **Rujukan tetap.** Setiap sesi agent yang menyentuh transfer IGD → Rawat Inap membaca berkas ini lebih dulu dan **tidak** menyimpulkan ulang akar masalah atau arah perbaikan |
| Dasar | `IGD-DEC-304`…`309`, `318`…`323`; `IGD-FACT-123`…`129`; `IGD-OQ-132`; permintaan ke Rawat Inap [`IGD-REQ-003`](../approval-requests/2026-10-18-permintaan-pemilik-rawat-inap-transfer-igd.md) |
| Bukti | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-b6-20261018/` (lokal, di-gitignore) dan `NewQuilvianSystemBackend/Logs/quilvian-backend-20261018.json`; laporan penguji [2026-10-18-laporan-uji-putaran-b6.md](../testing/2026-10-18-laporan-uji-putaran-b6.md) |
| Snapshot source | Backend `rizkiG` `6bb18fe6`, frontend `RizkiV2` `58ee55d32` |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Putaran B6 membuktikan seluruh jalur IGD sampai pasien di-_admit_ ke rawat inap: form _Selesai_, pembatalan sesudah
diproses ditolak, dan pasien tanpa form tidak masuk antrean. Satu hal gagal: **kunjungan IGD tidak tertutup** walaupun
pasien sudah di-_admit_ lalu tiba di bangsal (B6-06). Penyebabnya ada **di IGD**: _admit_ milik Rawat Inap mengubah
tindak lanjut menjadi _Executed_ persis seperti kontraknya, tetapi IGD tidak pernah memindahkan kunjungannya ke status
`Disposed`, padahal penutupan IGD mensyaratkan status itu. Perbaikannya dikerjakan IGD (`BE-IGD-083`) **tanpa mengubah
kode Rawat Inap**, ditambah satu panggilan dari Rawat Inap yang sudah disepakati sejak 15 Oktober (`IGD-DEC-308`) tetapi
belum masuk blueprint Rawat Inap.

_Contoh._ Tn. A di-_admit_ pukul 08.00; tindak lanjut IGD-nya menjadi _Dijalankan_. Pukul 08.22 perawat bangsal menekan
_Catat Tiba_. Seharusnya kunjungan IGD Tn. A _Selesai_. Kenyataannya kunjungan tetap "Menunggu keputusan" dan tidak bisa
ditutup.

---

## 2. Hasil B6 per skenario

Disiplin uji bersih: tanpa lencana `next dev` (11 PNG), tanpa PNG kembar, tanpa sandi tertulis (19 skrip), tanpa SQL;
HEAD backend, HEAD frontend, DLL (2026-10-17 11:43:00 UTC), dan `BUILD_ID` `vyZON79AuJIJU924yT7Hf` sama di awal dan akhir.

| ID | Putusan | Dasar |
| --- | --- | --- |
| B6-00 | PASS | Build produksi, lima akun tanpa `403` |
| B6-01 | PASS | Form, minta TTD, tanda tangan `200`; pasien di antrean |
| B6-02 | PASS (opsi 2) | Opsi 1 belum terpasang: akun admisi ditolak `403` saat _admit_ (07.49). _Admit_ oleh SuperAdmin: `422` (07.54) → master **KELAS II** `isForInpatient` diubah `false` → `true` (tercatat `persiapan.json`, `IGD-DEC-322`) → `499` (07.58) → **`201`** (08.00.47) |
| B6-03 | PASS | Tindak lanjut _Dijalankan_; form _Selesai_ dengan keterangan admisi; tanpa _Batalkan Form_; _Cetak_ ada; `displayStatus` 4 |
| B6-04 | PASS | Sempat satu panggilan `…/undefined/cancel` `404` (salah skrip); lalu `409` _"Petugas admisi sudah memproses pasien ini dari Antrean Transfer IGD/Poli. Form tidak dapat dibatalkan dari IGD."_ dan `400` _"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."_ |
| B6-05 | PASS | Dokter tidak punya aksi _Batalkan_ pada tindak lanjut yang sudah dijalankan |
| **B6-06** | **FAIL — cacat nyata** | Log: `emergency-departures` `201`, `submit-handover` `200`, `depart` `200` (perawat IGD, 08.21–08.22); `accept-handover` `200`, `arrive` `200` (perawat ruangan, 08.22). Kunjungan tetap `visitStatus` 6 (`AwaitingDisposition`); PNG B6-03 dan B6-06 berlencana "Menunggu keputusan" |
| B6-07 | PASS | Pasien bertanda tanpa form tidak di antrean; rincian `404` |

---

## 3. Akar masalah B6-06 (diverifikasi di source)

```text
Jalur IGD biasa (tombol Jalankan)                 Jalur transfer (admit Rawat Inap)
---------------------------------                 ---------------------------------
EmergencyDispositionController :406-450           InpAdmissionTransferService.cs :156-162
  1. kunjungan -> AwaitingDisposition (bila perlu)  1. EmgDisposition.Status = Executed   <- sesuai BE-RWI-207
  2. kunjungan -> Disposed  (TryApplyVisitStatus)   2. (tidak ada langkah kunjungan)
  3. disposition = Executed                         3. SaveChanges
  4. TryCloseAfterDispositionAsync
                                                  Pasien tiba (arrive, IGD)
                                                    -> TryCloseAfterBlockerResolvedAsync
                                                    -> TryCloseAfterDispositionAsync (EmergencyVisitService.cs:679)
                                                    -> ValidateVisitClosureAsync (EmergencyDispositionService.cs:129)
                                                       "Kunjungan hanya dapat diselesaikan setelah
                                                        keputusan tindak lanjut ditetapkan."  <- status bukan Disposed
```

Sejak `IGD-DEC-304`, tindak lanjut rawat inap tidak lagi dijalankan dari IGD, sehingga langkah 1–2 di kiri **tidak
diambil alih siapa pun**. Akibatnya:

1. Penutupan alur normal (admisi dulu, lalu pasien tiba) tidak pernah terjadi.
2. `EvaluasiPenutupanKunjunganSusulanAsync` (`EmergencyDispositionService.AdmissionRequest.cs:1547`, `BE-IGD-077`) melewati
   pemeriksaan yang sama, jadi **juga gagal** walaupun kelak dipanggil Rawat Inap.
3. Lencana kunjungan tetap "Menunggu keputusan" sesudah admisi.

> **Koreksi 18 Oktober 2026 (siang).** Baris "Pasien tiba (arrive, IGD) → `TryCloseAfterBlockerResolvedAsync`" pada diagram
> di atas tidak tepat: `POST …/arrive` **tidak** mencoba penutupan. Pada B6-06 penutupan dicoba saat _Terima Dokumen_
> (08.22.21), lima detik sebelum _Catat Tiba_. Rincian dan dua temuan lain yang mengubah lingkup `BE-IGD-083` ada di
> bagian 9.

---

## 4. Apa kata blueprint Rawat Inap

| Sumber (folder `rawat-inap/episode-rawat-inap/`) | Isi |
| --- | --- |
| `contracts/admission-transfer-repair-006.md` baris 13 | _"`Executed` berarti permintaan telah dipakai untuk membuka admisi dan memesan bed. Penutupan fisik IGD tetap melalui departure/arrival/handover existing."_ |
| `contracts/state-transition-matrix.md`, amandemen EPS-006 | _"IGD Confirmed -> Executed hanya setelah admisi dan reservasi berhasil commit … departure/arrival/handover berjalan melalui owner existing."_ |
| `roadmap/backend-roadmap-admisi-transfer-igd.md` baris 22 | _"Pasien tidak boleh berstatus 'Menunggu Kamar' lagi di IGD setelah admisi selesai."_ |
| `BE-RWI-207` | _Admit_ membuka episode dan mengubah `EmgDisposition` menjadi `Executed` dalam satu transaksi |
| Seluruh blueprint Rawat Inap | **Tidak** memuat `IGD-DEC-308`/`309`: tidak ada panggilan evaluasi penutupan sesudah _admit_, tidak ada _Tolak_. [Spesifikasi 15 Oktober](2026-10-15-spesifikasi-serah-terima-rawat-inap-igd-dec-308-309.md) sudah diserahkan tetapi belum diadopsi |

---

## 5. Kepemilikan cacat

| Masalah | Milik | Penjelasan |
| --- | --- | --- |
| B6-06 — kunjungan tidak tertutup sesudah _admit_ lalu pasien tiba | **IGD sepenuhnya** | Rawat Inap bekerja sesuai kontraknya; kontrak itu menyerahkan penutupan ke mekanisme IGD, dan mekanisme IGD tidak mengenali tindak lanjut yang dijalankan admisi |
| `BE-IGD-077` acceptance 4 — bagian fungsi | **IGD** | Fungsi jatuh pada pemeriksaan `Disposed` yang sama |
| `BE-IGD-077` acceptance 4 — bagian pemanggil | **Rawat Inap, menggantung** | Panggilan sesudah _admit_ disepakati (`IGD-DEC-308`) tetapi belum ada di blueprint maupun kode Rawat Inap (`IGD-OQ-132`) |
| Lencana "Menunggu keputusan" sesudah admisi | IGD, dipicu panggilan Rawat Inap | Tanpa panggilan, IGD tidak tahu kapan admisi terjadi sebelum pasien tiba |

---

## 6. Arah perbaikan yang disepakati (`IGD-DEC-323`)

Pemilik memutuskan perbaikan **wajib sejalur dengan blueprint Rawat Inap**, supaya perbaikan berikutnya tidak harus
dikerjakan dua kali.

| Arah | Isi | Status |
| --- | --- | --- |
| **A — IGD, `BE-IGD-083`** | Promosi bertahap kunjungan ke `Disposed` dipindah dari controller ke satu method service IGD, lalu dipakai di tombol _Jalankan_, di `TryCloseAfterDispositionAsync` (bila ada tindak lanjut _Executed_ tetapi kunjungan belum `Disposed`; promosi tetap disimpan walau penutupan masih tertahan), dan di _Selesaikan_ manual. Lewat `TryApplyVisitStatus`; nol berkas Rawat Inap, nol `Program.cs`, nol migration. Butuh satu baris tambahan matriks state IGD yang disetujui pemilik | **Dipakai.** Sejalur: Rawat Inap tetap menulis _Executed_ seperti `BE-RWI-207`; penutupan tetap lewat "owner existing" |
| **B — Rawat Inap, satu panggilan** | Sesudah `EmgDisposition` menjadi _Executed_ di transaksi _admit_, panggil `EvaluasiPenutupanKunjunganSusulanAsync(visitId, actorUserId, now, ct)` sebelum `SaveChangesAsync` | **Dipakai, milik Hamzah.** Sejalur bila dimasukkan ke kontrak EPS-006 dan delta `BE-RWI-207`; diminta lewat `IGD-REQ-003`. Hanya cara ini yang memenuhi harapan baris 22 roadmap Rawat Inap dan alur pasien-tiba-dulu |
| Rawat Inap berhenti menulis `EmgDisposition` dan menggantinya dengan fungsi IGD | — | **Ditarik.** Membongkar desain `BE-RWI-207` dan kontrak EPS-006 |
| Penyapu berkala di IGD yang menutup kunjungan tanpa panggilan Rawat Inap | — | **Tidak disarankan.** Menyimpang dari `IGD-DEC-308` dan membuat IGD mengawasi tulisan modul lain |

Urutan: A lebih dulu, sehingga fungsi IGD sudah benar ketika Rawat Inap menambahkan B — panggilan itu langsung berfungsi
tanpa kerja ulang di kedua sisi.

---

## 7. Batas untuk agent berikutnya

1. Jangan mengubah berkas Rawat Inap dari sesi IGD. Permintaan ke Rawat Inap lewat `IGD-REQ-003`.
2. Jangan mengusulkan Rawat Inap berhenti menulis `EmgDisposition`, dan jangan membuat penyapu berkala.
3. Jangan menulis status kunjungan pada jalur baca (daftar, rincian, lencana); promosi hanya pada jalur tulis.
4. Promosi selalu lewat penjaga `TryApplyVisitStatus`; bila ditolak (misalnya observasi masih aktif), kembalikan alasan
   yang jelas — jangan dipaksa.
5. Jalur _Jalankan_ IGD harus berperilaku persis seperti sebelumnya.
6. Kunjungan yang terlanjur tertahan (P-B6-A, `IGD-261018003352-28FF6F`) dirapikan sekali lewat _Selesaikan_ sesudah
   `BE-IGD-083`, bukan lewat SQL.

---

## 8. Akibat pada task (dicatat oleh sesi berikutnya)

| Task | Status yang ditetapkan |
| --- | --- |
| `BE-IGD-074` | ✅ 10 dari 10 — #3 rincian `404` (B5-10, B6-07) dan _admit_ memakai `GetTransferByIdAsync` → `TransferQuery` yang sama sehingga `404` (`InpAdmissionTransferService.cs` baris 89); #5 B6-04; #6 B6-03 |
| `FE-IGD-061` | ✅ — #2 keadaan _Selesai_ (B6-03); #3 bagian sesudah _admit_ (B6-03, B6-05, B6-04); #5 (B6-03) |
| `BE-IGD-077` | 🟡 — acceptance 4 cacat di source (bagian IGD → `BE-IGD-083`); pemanggil menunggu `IGD-OQ-132` / `IGD-REQ-003` |
| `BE-IGD-083` | Baru — disusun lewat `plan-module-delivery`; ⛔ sampai pemilik menyetujui baris matriks state |
| Uji | Putaran B7: ulang B6-06 sesudah `BE-IGD-083`; alur pasien-tiba-dulu sesudah Rawat Inap menambahkan panggilan B |

---

## 9. Koreksi dan tambahan — 18 Oktober 2026 (siang)

Ditemukan saat menyusun kartu `BE-IGD-083` lewat `plan-module-delivery`, dari source backend `6bb18fe6` dan
`Logs/quilvian-backend-20261018.json`. **Arah perbaikan bagian 6 tidak berubah** — tetap IGD (arah A) ditambah satu panggilan
Rawat Inap (arah B), dan Rawat Inap tetap menulis _Executed_ seperti `BE-RWI-207`. Yang bertambah adalah isi arah A.

| ID | Temuan | Bukti | Akibat pada `BE-IGD-083` |
| --- | --- | --- | --- |
| `IGD-FACT-130` | **_Catat Tiba_ bukan titik pemicu penutupan.** `EmergencyDepartureController.Arrive` memanggil `ExecuteDeparture` tanpa `cobaPenutupanSusulan`; yang mencoba penutupan hanya _Terima_/_Tolak Dokumen_, batal kepergian, dan sikap pesanan (state §9.2). Pada B6-06, _Terima Dokumen_ 08.22.21 terjadi saat pasien belum tiba, lalu _Catat Tiba_ 08.22.26 tidak mencoba lagi. Contoh di `IGD-FACT-121` ("tertutup saat tiba") hanya benar bila _Catat Tiba_ mendahului _Terima Dokumen_ | Source `EmergencyDepartureController.cs:165-170`, `:236-256`; log B6 | Delta baris 2: _Catat Tiba_ menjadi titik pemicu ke-5. Tanpa ini, promosi saja tidak membuat B6-06 lulus |
| `IGD-FACT-131` | **Fungsi evaluasi tidak melihat _Executed_ yang belum disimpan.** `TryCloseAfterDispositionAsync` mencari tindak lanjut _Executed_ dengan kueri basis data `AsNoTracking`. Panggilan Rawat Inap yang diletakkan sebelum `SaveChangesAsync` (`IGD-REQ-003` butir 1) terjadi saat perubahan _Executed_ masih di memori, sehingga tidak ditemukan dan penutupan dilewati diam-diam | Source `EmergencyVisitService.cs:690-701`, `InpAdmissionTransferService.cs:156-164` | Cakupan no. 3(a): tindak lanjut dicari lebih dulu di entity yang sedang dilacak `DbContext`. Posisi panggilan Rawat Inap tidak perlu diubah |
| `IGD-FACT-132` | **Tanpa penahan tambahan, promosi akan menutup kunjungan alur normal saat _admit_.** Penjaga penutupan hanya menahan kepergian yang **belum** tiba; bila belum ada kepergian sama sekali, tidak ada yang menahan. Begitu Rawat Inap memanggil evaluasi sesudah _admit_ dan kunjungan sudah dipromosikan, kunjungan yang pasiennya masih di IGD langsung `Completed`. Sesudah itu kepergian tidak dapat dibuat (_"EmergencyVisitId tidak ditemukan atau kunjungan sudah ditutup."_) dan catatan klinis yang belum ditandatangani terkunci (`BE-IGD-051`). Spesifikasi 15 Oktober §2.2 dan state §11.6.2 sebenarnya sudah mensyaratkan pasien tiba lebih dulu | Source `EmergencyDispositionService.cs:125-175`, `EmergencyDepartureService.cs:327-337` | Delta baris 3: tindak lanjut bertanda `RequiresAdmissionRequest` baru boleh ditutup bila ada kepergian `Arrived` |

Ketiga baris delta beserta rinciannya ada di kartu `BE-IGD-083` (bagian *Delta*). **Disetujui pemilik 18 Oktober 2026**
(`IGD-DEC-324`, menjawab `IGD-OQ-133`) dan masuk state `0.15.0` §11.8 serta validation `0.19.0` §14.10. Bagian 7 tetap berlaku; tambahannya:

7. Ketiga baris delta dirilis bersama. Promosi tanpa penahan baris 3 akan menutup kunjungan saat _admit_ begitu panggilan
   Rawat Inap ada.
8. Jangan meminta Rawat Inap memindahkan panggilannya ke sesudah `SaveChangesAsync`; fungsi IGD yang menyesuaikan
   (`IGD-FACT-131`).

Tabel bagian 8 diperbarui: `BE-IGD-083` siap untuk `build-module-backend` sesudah **delta tiga baris** (bukan satu baris) disetujui;
`BE-IGD-077` juga menunggu `BE-IGD-083`.
