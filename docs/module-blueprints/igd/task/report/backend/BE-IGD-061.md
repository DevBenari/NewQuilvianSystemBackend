# Laporan Perubahan Backend — `BE-IGD-061`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-061` |
| Judul | Penutupan menyusul dari tiga titik pemicu, dan observasi yang diakhiri sesudah disposisi dilaksanakan |
| Slice | `S5` · `EPIC IGD-13` · `MVP-8` |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian R3.14 |
| Trace | `FR-IGD-088`, `092`, `093`; `IGD-DEC-165`, `136`, `171`, `172`, `173`; approval `IGD-DEC-170`, `175` (sementara, `IGD-DEC-174`); `AT-IGD-187`, `188`, `192`, `193` |
| Contract version | validation `0.10.0` §11 aturan 4–7 dan §11.1 aturan 12–15 beserta urutan pemeriksaannya; state `0.7.0` §9.2 dan §9.5; API `0.13.0` §9.1 nomor 4–5 dan §9.4 — `approved`. Hash kelima berkas kontrak cocok dengan manifest 0j, 0j.1, 0j.2 |
| Dependency | `BE-IGD-060` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 0 (3 berkas), logika bisnis 2 (penutupan sebagai efek samping, urutan penolakan, transaksi), kontrak API 2 (perilaku `409`/`200` berubah), database 1, keamanan/auth 0, UI/workflow 1 |
| Task mode | `BACKEND` — wewenang pemilik 1 Oktober 2026 (*"Sesudah itu R3.14 (`BE-IGD-061`/`062`/`063`, `FE-IGD-041`/`042`)"*). Tanpa `dotnet build`, migration, `Program.cs`, eksekusi database, commit |
| Target tulis | `NewQuilvianSystemBackend`: source IGD dan `docs/module-blueprints/igd/` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b9076c71` (`rizkiG`) + working tree `BE-IGD-053`, `057`, `058`, `059` yang belum di-commit |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 2 Oktober 2026.** Build pemilik terbukti. Uji API: **6 terbukti** (S1, S5, S7, S8, S9, S10), **2 sebagian** (S4, S11), **4 belum terbukti** (S2, S3, S6, S12). Pemicu lewat observasi dan lewat pembatalan kepergian terbukti; pemicu `accept-handover`, `reject-handover`, dan sikap pesanan **belum** — tertahan `403` kewenangan unit sebelum sampai ke kode task ini. Kriteria 8 (observasi Dieskalasi pada kunjungan `Disposed`) belum terbukti lewat API. *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete.** Tiga berkas source; QBE checker `PASS`. **Belum:** build pemilik dan uji API S1–S12 (bagian 5.1) |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` backend; `rules/backend/` suite 1.17.1; kontrak rekayasa dan registry |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, `Emg`, `ACTIVE / LEGACY`. Nol entity baru |
| Keberlakuan | `NEW CODE`: `EmergencyVisitService.TryCloseAfterBlockerResolvedAsync`. `TOUCHED LEGACY`: `EmergencyObservationController.UpdateObservationStatus`, enam action `EmergencyDepartureController` |
| Branch | `rizkiG`, upstream `origin/rizkiG` |
| QBE yang berlaku | `QBE-SVC-001` (orkestrasi transaksi dan penyimpanan di service; controller kepergian tetap tanpa context), `QBE-TXN-001` (aksi pemicu dan penutupan dalam satu transaksi), `QBE-API-001`, `QBE-PERM-001` (metadata akses tidak berubah), `QBE-VAL-001`, `QBE-LOG-001` (pelaku penutupan tercatat) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*`, `QBE-CODE-*`, `QBE-DTO-001` (bentuk request dan response tidak berubah) |
| Checker QBE | Strict, working tree: 0 `VIOLATION`, 0 `REVIEW`, `PASS` |

---

## 1. Masalah yang diperbaiki

Sejak `BE-IGD-060`, menandai tindak lanjut pasien *dilaksanakan* menutup kunjungannya — kecuali masih ada kewajiban
klinis yang belum tuntas. Dua celah tersisa:

| Celah | Akibat |
| --- | --- |
| Kunjungan yang tertahan tidak pernah tertutup sendiri | Sesudah kewajiban terakhir dibereskan, kunjungan tetap terbuka sampai ada yang ingat menekan *selesaikan* |
| Observasi yang masih berjalan sesudah tindak lanjut dilaksanakan **tidak dapat diselesaikan** | Menyelesaikannya ditolak `409` (kunjungan `Disposed` tidak boleh mundur); satu-satunya jalan membatalkan observasi, sehingga kesimpulannya hilang |

*Contoh.* Pasien pulang pukul 14.00, tindak lanjutnya ditandai dilaksanakan, tetapi observasinya lupa ditutup. Pukul
15.00 perawat menyelesaikan observasi dengan kesimpulan *"tanda vital stabil"*. Sebelum task ini: `409`. Sesudahnya:
kesimpulan tersimpan dan kunjungan tertutup saat itu juga atas nama perawat pukul 15.00.

---

## 2. Proses bisnis

**Pelaku.** Petugas yang membereskan kewajiban terakhir — perawat (observasi), perawat unit penerima (serah
terima), atau petugas yang menetapkan sikap pesanan.

**Tiga titik pemicu.** Sesudah aksinya tersimpan, sistem mencoba menutup kunjungan:

| # | Aksi | Endpoint |
| ---: | --- | --- |
| 1 | Observasi diselesaikan atau dibatalkan pada kunjungan `Disposed` | `PATCH /emergency-observations/{id}/observation-status` |
| 2 | Serah terima diterima, ditolak, atau kepergian dibatalkan | `POST …/accept-handover`, `POST …/reject-handover`, `PATCH …/cancel` |
| 3 | Sikap atas pesanan ditetapkan, pesanan diterima atau ditolak | `PATCH …/order-items/{itemId}/action`, `POST …/accept`, `POST …/reject` |

**Urutan pada tiap pemicu.**

1. Aksi petugas divalidasi dan disimpan seperti biasa.
2. Bila kunjungannya punya tindak lanjut yang sudah dilaksanakan dan belum selesai, penjaga penutupan yang sudah ada
   dijalankan — tanpa diubah.
3. Penjaga lolos → kunjungan `Completed`, encounter ikut tertutup, penanda asal penutupan menunjuk tindak lanjut itu,
   pelakunya petugas yang baru saja beraksi.
4. Penjaga menolak (masih ada penahan lain) → aksi petugas **tetap berhasil**; kunjungan tetap menunggu.
5. Langkah 1–3 berada dalam **satu transaksi**: bila penutupan gagal disimpan, aksi petugas ikut batal.

**Observasi pada kunjungan yang tindak lanjutnya sudah dilaksanakan.**

| Tujuan | Yang terjadi |
| --- | --- |
| *Selesaikan* (dari aktif maupun dari tereskalasi) | Diterima. Status kunjungan tidak dipindahkan. Kesimpulan disimpan, lalu penutupan dicoba |
| *Eskalasi* | **Ditolak `409`** *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."* — observasi tetap aktif dan tetap menahan penutupan |
| *Batalkan* | Seperti semula, ditambah percobaan penutupan |
| *Aktifkan* | Seperti semula — ditolak penjaga kunjungan dengan pesannya sendiri |

**Urutan penolakan observasi:** `404` → `400` perpindahan status observasi tidak sah → `409` eskalasi pada kunjungan
`Disposed` → `400` catatan lebih dari 1000 karakter → simpan → penutupan.

*Contoh urutan.* Eskalasi dengan catatan 1.200 karakter pada kunjungan `Disposed` dijawab `409`, bukan `400`:
walaupun catatannya diringkas, eskalasinya tetap ditolak.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-IGD-061`; validation §11, §11.1; state §9.2, §9.5; API §9.1, §9.4.
Source: `EmergencyObservationController.cs`, `EmergencyDepartureController.cs`, `EmergencyDepartureService.cs`
(`UpdateHandoverAsync`, `CancelAsync`, `SetOrderActionAsync`, `SetOrderAcceptanceAsync`, `UbahFisikAsync`,
`UbahHandoverAsync`), `EmergencyDispositionService.ValidateVisitClosureAsync`, `EmergencyVisitService.cs`
(`TryCloseAfterDispositionAsync`, `ApplyEncounterClosureAsync`), `EmergencyDispositionController.UpdateDispositionStatus`
(pola `BE-IGD-060`), `ClinicalDocumentIntegrityService`, `EmgDeparture.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `…/Controllers/EmergencyObservationController.cs` | Penolakan eskalasi pada kunjungan `Disposed` sebelum pemeriksaan lain; *selesaikan* pada kunjungan `Disposed` tidak lagi memetakan ke `AwaitingDisposition`; sesudah observasi tersimpan, penutupan dicoba di dalam transaksi yang sama; log memuat hasil penutupan. +58/−3 baris |
| `…/Controllers/EmergencyDepartureController.cs` | Enam action — `accept-handover`, `reject-handover`, `cancel`, `order-items/{itemId}/action`, `accept`, `reject` — dijalankan lewat pembantu `DenganPenutupanSusulanAsync`; constructor + `EmergencyVisitService`; log penutupan. +99/−8 baris |
| `…/Services/EmergencyVisitService.cs` | + `TryCloseAfterBlockerResolvedAsync`: membuka transaksi, menjalankan aksi pemicu, mencoba penutupan, menyimpan, lalu menutup transaksi. +27 baris |

**Nol baris komentar ditambah** — jumlah baris komentar ketiga berkas sama dengan sebelum disunting. **Tidak
disentuh:** `EmergencyObservationService`, `EmergencyDepartureService`, `EmergencyDispositionService`, penjaga
penutupan, `Program.cs`, model, configuration, migration, frontend.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai kontrak terkunci (API §9.1 nomor 4–5, §9.4). Bentuk request dan response tidak berubah. `PATCH observation-status` pada kunjungan `Disposed`: *selesaikan* kini `200` (dulu `409`); *eskalasi* tetap `409` dengan kalimat baru |
| Database | Nol schema, nol migration. Penutupan menulis kolom yang sama dengan `BE-IGD-060` |
| Keamanan/Auth | Nol hak akses baru: petugas menutup kunjungan sebagai akibat aksi yang memang wewenangnya (permission §8) |

### 3.4 Selisih terhadap kartu dan keputusan pelaksanaan

| Butir | Isi |
| --- | --- |
| **Pemicu dijalankan sesudah aksi tersimpan, bukan sebelum `SaveChanges`** | Kartu menulis *"titik pemicu … sebelum `SaveChanges`"*. Penjaga penutupan membaca **basis data**, bukan perubahan yang masih di memori; observasi yang baru diselesaikan tetapi belum disimpan masih terbaca aktif, sehingga penjaga selalu menolak dan penutupan tidak pernah terjadi. Urutannya karena itu: simpan aksi → jalankan penjaga → simpan penutupan, seluruhnya dalam satu transaksi — memenuhi validation §11.1 aturan 13 (*"pada penyimpanan yang sama"*) |
| **`EmergencyVisitService` ikut disentuh** | Kartu menulis berkas itu tidak disentuh. Controller kepergian tidak memegang context basis data, dan memasukkannya membuat checker QBE memberi dua catatan `QBE-SVC-001`. Layanan kepergian tidak dapat memanggil layanan kunjungan (lingkaran dependensi: kunjungan → disposisi → kepergian). Satu method ditambahkan pada layanan kunjungan, yang sudah terdaftar — nol `Program.cs` |
| Pada observasi, penutupan hanya dicoba bila kunjungannya `Disposed` | Penjaga penutupan memang mensyaratkan `Disposed`, jadi hasilnya sama; jalur biasa tidak terbebani transaksi dan kueri tambahan (kriteria 4 dan 12) |
| Enam aksi kepergian kini berjalan di dalam transaksi | Sebelumnya masing-masing satu penyimpanan tanpa transaksi. Bila aksinya ditolak, transaksi dibatalkan — tidak ada yang tersimpan sebelum penolakan pada keenam jalur itu, jadi perilaku penolakannya tidak berubah |
| **`arrive` tidak dipasangi pemicu** | Kontrak (state §9.2, validation §11 aturan 4) menyebut tiga aksi serah terima, dan kartu mengikutinya. Padahal penahan *"masih ada proses kepergian yang belum selesai"* dilepas oleh **keadaan fisik** pasien (`arrive` atau `cancel`), bukan oleh serah terima. Akibatnya: bila serah terima diterima **sebelum** pasien dicatat tiba, pencatatan tiba sesudahnya tidak menutup kunjungan. Kunjungan itu tetap dapat ditutup manual dan tetap tampil pada saringan menunggu penutupan. Menambah `arrive` sebagai pemicu adalah perubahan kontrak — keputusan pemilik |

---

## 4. Dokumentasi endpoint

Seluruhnya endpoint yang sudah ada; yang bertambah hanya efek samping penutupan kunjungan.

#### Health Services / Emergency Installation Management / Emergency Observation

Base URL: `api/v1/health-services/emergency-installation-management/emergency-observations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/observation-status` | Mengubah status observasi; pada kunjungan `Disposed`: *selesaikan* diterima dan penutupan dicoba, *eskalasi* ditolak `409` | `EmergencyObservation : Update` (tidak berubah) |

#### Health Services / Emergency Installation Management / Emergency Departure

Base URL: `api/v1/health-services/emergency-installation-management/emergency-departures`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/accept-handover`, `/{id}/reject-handover` | Meninjau serah terima; penutupan kunjungan dicoba | `EmergencyDeparture : Update` (tidak berubah) |
| `PATCH` | `/{id}/cancel` | Membatalkan kepergian; penutupan kunjungan dicoba | `EmergencyDeparture : Update` (tidak berubah) |
| `PATCH` | `/{id}/order-items/{itemId}/action` | Menetapkan sikap pesanan; penutupan kunjungan dicoba | `EmergencyDeparture : Update` (tidak berubah) |
| `POST` | `/{id}/order-items/{itemId}/accept`, `/reject` | Menerima atau menolak pesanan; penutupan kunjungan dicoba | `EmergencyDeparture : Update` (tidak berubah) |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker Strict (working tree) | 0 `VIOLATION`, 0 `REVIEW`, `Final result: PASS` | `PASS` | Keluaran perintah 1 Oktober 2026 |
| Review diff | Hanya perubahan task; akhiran baris tiap berkas dipertahankan; nol baris komentar ditambah | `PASS` | `git diff`; hitungan baris komentar sebelum dan sesudah sama |
| Lingkaran dependensi | Nol: controller kepergian → layanan kunjungan → layanan disposisi → layanan kepergian | `PASS` | Baca constructor ketiga layanan |
| `dotnet build` | — | `NOT RUN` | Milik pemilik |
| Uji API S1–S12 | — | `NOT RUN` | Menunggu build pemilik |

Uji manual: `REQUIRED`.

**Tidak dijalankan:** `dotnet build` (arahan pemilik), uji API (butuh build), eksekusi database.

### 5.1 Skenario uji untuk pemilik

Build: `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false`. Simpan **badan respons** tiap langkah.
"Menunggu penutupan" = kunjungan `Disposed` yang tindak lanjutnya `Executed` tetapi masih punya penahan.

| # | Skenario | Hasil yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| S1 | Kunjungan menunggu penutupan dengan **satu observasi aktif** sebagai satu-satunya penahan. `PATCH observation-status` `{ "observationStatus": 2, "notes": "tanda vital stabil" }` | `200`; observasi `Completed`, `completionSummary` terisi. `GET` kunjungan: `Completed`, `closedByDispositionId` terisi tindak lanjut itu, pelaku pembaruan = pengguna yang menyelesaikan observasi; encounter tertutup | 1 |
| S2 | Kunjungan menunggu penutupan karena kepergian yang belum tuntas: catat pasien tiba (`arrive`), lalu `POST accept-handover` | Sesudah `accept-handover`: `200`; kunjungan `Completed` | 2 |
| S3 | Kunjungan menunggu penutupan karena satu pesanan belum bersikap: `PATCH order-items/{itemId}/action` | `200`; kunjungan `Completed` | 3 |
| S4 | Aksi yang sama dengan S1–S3 pada kunjungan yang **tidak** menunggu penutupan (misalnya masih `InTreatment`) | Aksinya berhasil seperti biasa; status kunjungan mengikuti aturan lama; tidak tertutup | 4, 12 |
| S5 | Kunjungan menunggu penutupan dengan **dua** penahan (observasi aktif + pesanan belum bersikap): selesaikan observasinya | `200`; observasi `Completed`; kunjungan tetap `Disposed` | 5 |
| S6 | Observasi berstatus `Escalated` pada kunjungan `Disposed`: selesaikan | `200`; kunjungan tidak dipindahkan ke `AwaitingDisposition`; tertutup bila tak ada penahan lain | 8 |
| S7 | Kunjungan `Disposed` dengan observasi aktif: `{ "observationStatus": 3, "notes": "pasien memburuk" }` (eskalasi) | `409` *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."*; observasi tetap `Active`, `escalationReason` kosong, kunjungan tetap `Disposed` | 9 |
| S8 | S7 dengan catatan 1.200 karakter | `409` kalimat yang sama, **bukan** `400` | 10 |
| S9 | Kunjungan `Disposed`: selesaikan observasi dengan catatan 1.200 karakter | `400` *"Catatan paling banyak 1000 karakter."*; observasi dan kunjungan tidak berubah | 11 |
| S10 | Kunjungan `UnderObservation`: selesaikan observasi; kunjungan `InTreatment` lain: eskalasi observasi | Kunjungan pertama → `AwaitingDisposition`; kedua → tetap lewat penjaga seperti semula | 12 |
| S11 | Kunjungan menunggu penutupan: `POST reject-handover` (beralasan), dan pada kunjungan lain `PATCH cancel` kepergian | Aksi `200`; kunjungan tertutup bila itu penahan terakhir | 2, 6 |
| S12 | Kunjungan menunggu penutupan: `POST order-items/{itemId}/accept`, dan pada pesanan lain `…/reject` | Aksi `200`; kunjungan tertutup bila itu penahan terakhir | 3, 6 |

Nilai `observationStatus`: `Active` 1, `Completed` 2, `Escalated` 3, `Cancelled` 4.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Observasi diselesaikan dengan kesimpulan → `200`, kunjungan `Completed`, pelakunya petugas itu, `ClosedByDispositionId` terisi | **Terpenuhi pada source** | `UpdateObservationStatus`. Uji S1 belum |
| 2 | Serah terima diterima → kunjungan tertutup | **Terpenuhi pada source** | `AcceptHandover` lewat `ExecuteDeparture(…, true)`. Uji S2 belum |
| 3 | Sikap pesanan ditetapkan → kunjungan tertutup | **Terpenuhi pada source** | `SetOrderAction`, `SetOrderAcceptance`. Uji S3 belum |
| 4 | Aksi yang sama pada kunjungan yang tidak menunggu penutupan tidak berefek | **Terpenuhi pada source** | `TryCloseAfterDispositionAsync` melewati kunjungan tanpa tindak lanjut `Executed`. Uji S4 belum |
| 5 | Masih ada penahan lain → kunjungan tetap menunggu; observasi tetap `Completed` | **Terpenuhi pada source** | Penjaga menolak → hanya aksi yang disimpan. Uji S5 belum |
| 6 | Ketiga titik pemicu benar-benar dipasang — nol tertinggal | **Terpenuhi** | Satu action observasi + enam action kepergian; `git diff`. `arrive` sengaja tidak — lihat 3.4 |
| 7 | Build 0 error | **Belum** | Build milik pemilik |
| 8 | Observasi `Escalated` pada kunjungan `Disposed` diselesaikan → `200`, kunjungan tetap `Disposed` sebelum penutupan dicoba | **Terpenuhi pada source** | Pemetaan `Completed when !kunjunganDisposed`. Uji S6 belum |
| 9 | Eskalasi pada kunjungan `Disposed` → `409` kalimat aturan 14 persis; nol perubahan | **Terpenuhi pada source** | Penolakan sebelum entity disentuh; kalimat sama huruf demi huruf dengan validation §11.1 aturan 14. Uji S7 belum |
| 10 | Eskalasi dengan catatan 1.200 karakter → `409`, bukan `400` | **Terpenuhi pada source** | Penolakan eskalasi mendahului pemeriksaan catatan. Uji S8 belum |
| 11 | Selesaikan dengan catatan 1.200 karakter pada kunjungan `Disposed` → `400` | **Terpenuhi pada source** | Tanpa target status kunjungan, pemeriksaan batas catatan berlaku. Uji S9 belum |
| 12 | Perilaku pada kunjungan selain `Disposed` tidak berubah | **Terpenuhi pada source** | Seluruh cabang baru berada di balik `kunjunganDisposed`. Uji S10 belum |

DoD: laporan tracked ✅ (berkas ini). Belum ✅ karena kriteria 7 belum ada dan uji API belum dijalankan. `FE-IGD-042`
boleh **dikerjakan**; uji layarnya menunggu build task ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Penutupan terjadi sebagai efek samping. Pemanggil keenam endpoint kepergian dan endpoint observasi dapat mendapati kunjungan `Completed` pada pembacaan berikutnya |
| Masalah yang diketahui | (1) `arrive` bukan titik pemicu (3.4). (2) Observasi `Escalated` dapat tertinggal pada kunjungan `Completed` dan hanya dapat dibatalkan — di luar cakupan kartu (state §9.5). (3) Cara mencatat pasien yang memburuk sesudah tindak lanjutnya dilaksanakan belum diputuskan (`IGD-OQ-111`) |
| Risiko tersisa | Belum ada build. Enam aksi kepergian kini di dalam transaksi — bila salah satu method layanan kepergian kelak membuka transaksinya sendiri, keduanya akan bertabrakan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tiga berkas source berubah oleh task ini (`EmergencyVisitService.cs` bersama perubahan `BE-IGD-057`/`058`); laporan ini; status roadmap/traceability. Tanpa stage, commit, atau push |
| Langkah berikutnya | Pemilik: build, lalu S1–S12. Agent: `BE-IGD-062`, `063`, lalu `FE-IGD-042` |

---

## Pemeriksaan bukti uji gabungan — 2 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/` (`results-tahap-1.json`…`results-tahap-4.json`, skrip `test-tahap-*.mjs`, tangkapan layar `<ID>.png`) dan log backend `Logs/quilvian-backend-20261001.json`, `quilvian-backend-20261002.json`. Ringkasan agen penguji ([laporan uji gabungan](../../../testing/2026-10-01-laporan-uji-gabungan-r313-r314.md)) **tidak** dipakai sebagai bukti: uraian skenarionya pada beberapa task tidak sama dengan panduan, dan daftar `FAIL`-nya tidak cocok dengan JSON mentah.

Build pemilik terbukti dari artefak: `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 1 Oktober 2026 15.18, sesudah edit source terakhir (14.54); jumlah warning tidak dilaporkan. Source di-commit pemilik sebagai `74a72399`.

| Skenario | Putusan | Yang teramati pada bukti mentah |
| --- | --- | --- |
| `061-S1` | **Terbukti** | Observasi `200` → `Completed`, `completionSummary` terisi; kunjungan `visitStatus` 9; encounter `encounterStatus` 9 |
| `061-S2` | **Belum terbukti** | `arrive` dan `accept-handover` keduanya `403` *"Unit Rawat Inap belum dipetakan ke simpul organisasi…"*; kunjungan tetap 7. Pemicu tidak pernah tercapai |
| `061-S3` | **Belum terbukti** | Skrip gagal membuat pesanan: `POST order-items` `403` *"Unit asal belum tercatat…"*; `PATCH …/action` tidak pernah dikirim |
| `061-S4` | **Sebagian** | Hanya kaki observasi: selesai pada kunjungan `InTreatment` → `200`, kunjungan menjadi 6 (aturan lama), tidak tertutup. Kaki kepergian dan pesanan tidak dijalankan |
| `061-S5` | **Terbukti** | Dua penahan (observasi + kepergian): observasi `200`, kunjungan tetap 7 dengan alasan kepergian |
| `061-S6` | **Belum terbukti** | Prasyarat tidak terpenuhi. Observasi dieskalasi saat `InTreatment`, lalu tindak lanjut dijalankan: kunjungan **langsung `Completed`** karena periode Dieskalasi tidak menahan penutupan. Penyelesaian periode itu sesudahnya `409` *"Status kunjungan tidak dapat berubah dari Completed ke AwaitingDisposition."* |
| `061-S7` | **Terbukti** | `409` dengan kalimat `IGD-DEC-172` |
| `061-S8` | **Terbukti** | Catatan 1.200 karakter tetap `409` kalimat yang sama, bukan `400` |
| `061-S9` | **Terbukti** | `400` *"Catatan paling banyak 1000 karakter."* |
| `061-S10` | **Terbukti** | Kunjungan `UnderObservation` → 6; eskalasi pada `InTreatment` `200`. Bukti tipis: hanya nilai ringkas, tanpa badan respons |
| `061-S11` | **Sebagian** | `PATCH cancel` kepergian `200` → kunjungan `Completed` 9. Kaki `reject-handover` tidak dijalankan |
| `061-S12` | **Belum terbukti** | Sama dengan S3: `POST order-items` `403`; `accept` dan `reject` tidak pernah dikirim |

**Catatan.**

- **Bukan kegagalan kode task ini.** `403` berasal dari pemeriksaan kewenangan unit (`EmergencyUnitAuthorityService`, `IGD-DEC-092`): unit tujuan belum dipetakan ke simpul organisasi dan kepergian uji dibuat tanpa unit asal. Skenario baru dapat dibuktikan pada data yang unitnya terpetakan dan penggunanya ditugaskan ke unit itu.
- **Temuan untuk pemilik (aturan lama, bukan dari task ini).** Penjaga penutupan hanya menghitung observasi `Active`. Periode `Escalated` tidak menahan, sehingga kunjungan dapat `Completed` sementara periodenya masih Dieskalasi, dan periode itu tidak dapat diselesaikan lagi (`409`). `closedByDispositionId` tidak tampil pada respons `GET`, jadi tidak teramati.
- Kriteria 8 pada layar: `042-U6` memperlihatkan periode Dieskalasi pada kunjungan `Disposed` (ada penahan lain) beserta modalnya; hasil penyelesaiannya tidak dijalankan.

Putusan: **🟡 sebagian** — Build pemilik terbukti. Uji API: **6 terbukti** (S1, S5, S7, S8, S9, S10), **2 sebagian** (S4, S11), **4 belum terbukti** (S2, S3, S6, S12). Pemicu lewat observasi dan lewat pembatalan kepergian terbukti; pemicu `accept-handover`, `reject-handover`, dan sikap pesanan **belum** — tertahan `403` kewenangan unit sebelum sampai ke kode task ini. Kriteria 8 (observasi Dieskalasi pada kunjungan `Disposed`) belum terbukti lewat API.
