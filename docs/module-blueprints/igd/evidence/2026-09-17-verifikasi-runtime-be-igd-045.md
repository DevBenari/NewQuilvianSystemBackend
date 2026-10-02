# Verifikasi Runtime `BE-IGD-045` — dijalankan pemilik

| Field | Nilai |
| --- | --- |
| Tanggal | 17 September 2026 |
| Jenis | **Evidence pemilik.** Build dan uji API dijalankan Product/Domain Owner, bukan agent |
| Pelapor | Rizki Gunawan, Product/Domain Owner IGD |
| Task | `BE-IGD-045` |
| Backend | branch `rizkiG` `a5f4f4b8` + working tree |
| Cakupan | Build + dua belas skenario uji API. **Bukan UAT** |

---

## A. Build

```
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
Build succeeded
0 errors
207 warnings
```

Jumlah warning **sama persis** dengan build sebelum `BE-IGD-044` dan `BE-IGD-045` ada, jadi
ketiga berkas baru menyumbang **nol warning baru** — termasuk nol warning nullable dari
perubahan `AssignmentReason` menjadi `string?`.

## B. Dua belas skenario — seluruhnya `PASS`

| # | Hasil | Catatan pemilik | Acceptance |
| --- | :-: | --- | :-: |
| S1 | **PASS** | Assignment dokter pertama berhasil | 1, 12 |
| S2 | **PASS** | Assignment kedua tanpa handover ditolak `409` | 2 |
| S3 | **PASS** | Doctor GUID valid tetapi tidak ditemukan menghasilkan `400` business response | 1 |
| S4 | **PASS** | Missing, `null`, empty, dan whitespace `assignmentReason` seluruhnya menghasilkan pesan business yang sama | 4 |
| S5 | **PASS** | Handover valid berhasil | 3 |
| S6 | **PASS** | Histori dokter lama dan baru tetap tersimpan dengan boundary temporal yang sama | 3, 8 |
| S7 | **PASS** | `/active` mengembalikan dokter terbaru | 9 |
| S8 | **PASS** | `/active?at` mengembalikan dokter yang aktif pada interval yang benar | 9 |
| S9 | **PASS** | Waktu sebelum assignment pertama menghasilkan `404` | 9 |
| S10 | **PASS** | `EffectiveFrom` sebelum `ArrivalDateTime` ditolak `400` | 5 |
| S11 | **PASS** | `RegPatientEncounter.DoctorId` sinkron dengan dokter aktif terbaru | 7 |
| S12 | **PASS** | Dua `POST` paralel lewat `Promise.all` menghasilkan **satu `201` dan satu `409`**; basis data terverifikasi memiliki **tepat 1** `EmgDoctorAssignment` aktif | 6 |

### B.1 S4 menutup temuan nullability

Keempat bentuk masukan kosong menghasilkan pesan business yang sama. Itu membuktikan perbaikan
`AssignmentReason` menjadi `string?` benar-benar bekerja: `[Required]` implisit pada properti
reference type non-nullable tidak lagi menolak lebih dulu dengan `ProblemDetails` generik.

### B.2 S12 menutup acceptance concurrency dengan jalur yang benar

Dua permintaan **paralel** menghasilkan satu `201` dan satu `409` — bukan `500`. Artinya
`DbUpdateException` dari index unik bersyarat tertangkap `SimpanAsync` dan diterjemahkan menjadi
konflik terkendali, dan invarian *"tepat satu penugasan berjalan"* dijaga basis data, bukan
hanya oleh pemeriksaan service.

Ini jalur yang **berbeda** dari S2 yang berurutan, dan hanya skenario inilah yang membuktikan
`FR-IGD-019`.

## C. Dua observasi pemilik

### C.1 Wrapper response membawa `statusCode: 200` pada respons `201`

**Dilaporkan pemilik.** Penetapan yang berhasil membalas HTTP `201`, tetapi badan responsnya
memuat `"statusCode": 200`.

**Hasil audit — pola baseline aplikasi, bukan inkonsistensi `BE-IGD-045`.**

| Bukti | Isi |
| --- | --- |
| `Responses/ApiResponse.cs` | `Ok(...)` punya **satu** overload saja, dan menghardcode `StatusCode = StatusCodes.Status200OK`. Tidak ada jalan memberitahunya status lain |
| Pemakai `Status201Created` lain | Tersebar di `AccountingManagement` dan `HumanResource` — mereka memakai wrapper yang sama, jadi divergensinya identik |

Kesimpulan: setiap respons `201` di aplikasi ini membawa `statusCode: 200` pada badannya.
`BE-IGD-045` **tidak** memperkenalkan perilaku ini dan tidak menyimpang dari modul lain.

**Tidak diubah.** Memperbaikinya berarti menyentuh `ApiResponse<T>` yang dipakai seluruh
aplikasi — refactor lintas modul yang menuntut keputusan tersendiri, dan pemilik sudah melarang
mengubah global response wrapper tanpa evidence. Dicatat di sini sebagai temuan lintas modul,
bukan sebagai cacat task ini.

### C.2 Satu permintaan `/active` tercatat lambat sekitar 38 detik

**Dilaporkan pemilik.** Permintaan `/active` lain jauh lebih cepat.

**Dicatat sebagai observasi, bukan cacat.** Satu kejadian tunggal tanpa pengulangan belum cukup
menyimpulkan penyebab. Kandidat yang masuk akal dan **belum diuji**: kompilasi kueri EF pertama
kali pada proses yang baru dijalankan, atau jeda koneksi ke basis data pengembangan yang berada
di server remote.

**Tindak lanjut hanya bila dapat direproduksi.** Bila berulang pada permintaan yang sudah panas,
barulah ia layak diselidiki dengan ID task tersendiri.

## D. Status sesudah verifikasi ini

| Gerbang | Keadaan |
| --- | :-: |
| Implementation Complete | ✅ |
| Build Verified | ✅ |
| Runtime Verified | ✅ |
| UAT | **belum** — tidak diklaim |

## E. Yang **tidak** berubah karena verifikasi ini

| Hal | Keadaan |
| --- | --- |
| `BE-IGD-048` | ⛔ tetap terblokir `IGD-OQ-092` |
| `BE-IGD-039` | Task terpisah; pemilik tetap Security/Privacy owner, `OPEN` |
| Pemetaan 18 unit layanan | Task data terpisah; pemilik tetap Master Data, `OPEN` |
| Tabel `EmgDoctorAssignment` | Masih tanpa baris riwayat lama; celahnya berhenti bertambah begitu `FE-IGD-027` rilis |
