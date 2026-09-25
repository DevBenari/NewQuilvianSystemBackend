# `BE-IGD-047` — Nomor urut penilaian triage ditetapkan server

| Field | Nilai |
| --- | --- |
| Task | `BE-IGD-047` |
| Gelombang | R3.10 — 17 September 2026 |
| Status | ✅ **SELESAI — 17 September 2026.** Akar masalah terbukti; `dotnet build` lulus dan **uji API tiga skenario lulus** dijalankan pemilik; dibuktikan lagi lewat layar bersama `FE-IGD-033`. Tanpa UAT |
| Pemicu | Uji layar pemilik 17 September 2026: `409` saat **Simpan Pemeriksaan** pada triage IGD |
| Backend | branch `rizkiG`, di atas `2220ee81` |
| Kontrak | **Nol perubahan versi.** Bentuk request tidak berubah; tiga ruas menjadi usang-tetapi-diterima, pola yang sama dengan `recordedByUserId` pada `BE-IGD-046` |
| Migration | **Nol.** Tidak ada kolom, index, maupun constraint yang berubah |

---

## 1. Akar masalah — terbukti, bukan dugaan

`CreateEmergencyTriageRequest.Sequence` punya **nilai bawaan `1`**:

```csharp
public int Sequence { get; set; } = 1;   // sebelum perbaikan
```

Sementara controller memutuskan sumber nomor urut dengan:

```csharp
var nextSequence = request.Sequence > 0     // 1 > 0 → SELALU benar
    ? request.Sequence                       // → selalu 1
    : (hitung dari basis data) + 1;          // → tidak pernah dijalankan
```

Pemanggil yang **tidak** mengirim `sequence` — dan layar triage IGD memang tidak mengirimnya —
tetap membuat controller membaca `1`. Cabang penghitungan di server karena itu **kode mati sejak
awal**, dan setiap penilaian yang disimpan selalu memakai nomor urut `1`.

Akibatnya berurutan dan pasti:

| Penilaian ke- | Nomor urut yang dipakai | Hasil |
| ---: | ---: | --- |
| 1 | 1 | ✅ tersimpan |
| 2 | 1 | ⛔ `23505` pada `IX_EmgTriage_EmergencyVisitId_Sequence` |
| 3 | 1 | ⛔ sama, selamanya |

**Setiap kunjungan IGD hanya pernah bisa menyimpan satu penilaian triage.** Penilaian kedua —
termasuk pengkajian susulan yang sah — selalu ditolak.

### 1.1 Rantai bukti

| Bukti | Isi |
| --- | --- |
| Log `Logs/quilvian-backend-20260917.json` | Dua `23505` pada `IX_EmgTriage_EmergencyVisitId_Sequence`, keduanya dari `EmergencyTriageController.Create`, pukul **09:44:15** dan **09:58:52** WIB |
| Kueri pemilik, tabel `EmgTriage` | Kunjungan `f72af81a-bae4-4cec-8b32-f37901757a02` berisi **tepat satu** baris: `Sequence` = 1, `IsDelete` = false, dibuat **09:41:42** WIB |
| Kueri agregat pemilik | **Nol** kunjungan yang punya baris terhapus bernomor lebih tinggi daripada baris hidup |
| Pembacaan source | `EmergencyTriageDtos.cs:59` bawaan `= 1`; `EmergencyTriageController.cs:217` cabang `> 0` |

Ketiganya konsisten satu sama lain: satu baris sukses pukul 09:41, lalu dua penolakan yang
**tidak** menambah baris apa pun. Itu hanya mungkin bila nomor urut yang dicoba selalu `1`.

### 1.2 Dugaan yang dibantah bukti, dan dicatat supaya tidak diulang

Dugaan pertama — baris ter-soft-delete menempati nomor urut karena penghitung menyaring
`IsDelete` sedangkan index uniknya tidak — **salah untuk kejadian ini**. Kueri agregat pemilik
mengembalikan nol baris.

Ketidaksejajaran filter itu **tetap nyata** dan tetap diperbaiki di sini, tetapi statusnya
turun dari *penyebab* menjadi *pencegahan*: ia belum pernah menyala, dan akan menyala begitu
baris triage pertama di-soft-delete.

---

## 2. Yang diubah

| # | Berkas | Perubahan |
| ---: | --- | --- |
| 1 | `DTOs/EmergencyTriageDtos.cs` | Bawaan `Sequence = 1` **dicabut**. `Sequence`, `IsRetriage`, `PreviousTriageId` ditandai usang — tetap diterima, diabaikan sepenuhnya |
| 2 | `Controllers/EmergencyTriageController.cs` — `Create` | Cabang `request.Sequence > 0` dicabut. Nomor urut, penanda penilaian ulang, dan penunjuk pendahulu ditetapkan server lewat `HitungUrutanPenilaianBerikutnyaAsync` |
| 3 | `Controllers/EmergencyTriageController.cs` — helper baru | Nomor urut dihitung **tanpa** menyaring `IsDelete`, sejajar dengan index unik yang juga tanpa filter. Penunjuk pendahulu tetap dibaca dari baris yang masih berlaku |
| 4 | `Controllers/EmergencyTriageController.cs` — simpan | Tabrakan nomor urut diulang **satu kali** dengan nomor yang dihitung ulang; klik bersamaan tidak lagi menjadi galat bagi perawat |
| 5 | `Controllers/EmergencyTriageController.cs` — penolakan | Pesan duplikat dipisahkan dari pesan foreign key, dan kegagalan simpan kini mencatat `EmergencyVisitId` serta nomor urut yang dicoba |
| 6 | `Controllers/EmergencyTriageController.cs` — `Update` | Berhenti menimpa `Sequence`, `IsRetriage`, dan `PreviousTriageId` |
| 7 | `Services/EmergencyTriageService.cs` — retriage | Perbaikan 3 dan 5 diterapkan pada jalur penilaian ulang |

### 2.1 Mengapa perubahan 6 wajib ikut

`UpdateEmergencyTriageRequest` **mewarisi** `CreateEmergencyTriageRequest`, dan jalur `Update`
menimpa ketiga ruas itu dari request. Dengan bawaan lama `= 1`, satu `PUT` tanpa `sequence`
sudah cukup memindahkan penilaian mana pun ke nomor urut `1`. Mencabut bawaannya **tanpa**
memperbaiki `Update` justru memindahkannya ke `0` — lebih buruk, karena urutan riwayat menjadi
tidak terbaca.

`Update` karena itu berhenti menyentuh ketiganya sama sekali.

---

## 3. Kesesuaian dengan acceptance yang sudah terkunci

Laporan `BE-IGD-004` mengunci: *"Ketiganya ditetapkan server, **tidak dapat dikirim
pemanggil**"* untuk `Sequence`, `IsRetriage`, dan `PreviousTriageId`. Komentar dokumentasi
`RetriageEmergencyTriageRequest` menuliskan aturan yang sama.

`Create` dan `Update` **tidak** mematuhinya sampai hari ini. Perubahan ini menutup pelanggaran
itu — **bukan** mendesain requirement baru.

---

## 4. Perubahan perilaku yang perlu dinilai pemilik

| # | Perubahan | Sebelumnya |
| ---: | --- | --- |
| 1 | `IsRetriage` diisi server: benar bila kunjungan sudah punya penilaian yang masih berlaku | Diambil dari request; layar tidak pernah mengirimnya, jadi selalu `false` |
| 2 | `PreviousTriageId` diisi server: penilaian terakhir yang masih berlaku pada kunjungan itu | Diambil dari request; selalu kosong |
| 3 | `Update` tidak lagi dapat mengubah ketiga ruas itu | Dapat, dan menimpanya diam-diam |

Butir 1 dan 2 **memperbaiki data yang selama ini kosong** — rangkaian riwayat penilaian kini
benar-benar terbentuk. Keduanya mengikuti definisi yang sudah dipakai jalur retriage, jadi tidak
ada tafsir baru. Tetap perlu dilihat pemilik saat uji layar.

---

## 5. Validasi

| Jenis | Hasil |
| --- | --- |
| Keseimbangan struktur ketiga berkas | ✅ diperiksa, seimbang |
| `dotnet build` | ✅ **LULUS 17 September 2026** — dijalankan pemilik |
| Uji API | ✅ **LULUS 17 September 2026** — ketiga skenario bagian 6, dijalankan pemilik |
| Uji lewat layar | ✅ **LULUS 17 September 2026** — bersama `FE-IGD-033` |
| Automated test | Tidak ada — proyek test backend dihapus 11 September 2026 (`IGD-DEC-110`) |

## 6. Tiga skenario uji yang membuktikan perbaikan ini

Dijalankan pada satu kunjungan IGD yang **sudah punya** satu penilaian triage.

| # | Langkah | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | Simpan penilaian triage kedua pada kunjungan yang sama | `200`. Baris baru bernomor urut `2`, `isRetriage` benar, `previousTriageId` menunjuk penilaian pertama |
| 2 | Simpan penilaian ketiga | `200`, nomor urut `3`. Sebelum perbaikan, kedua langkah ini `409` |
| 3 | `PUT` penilaian tanpa ruas `sequence` | `200`, dan nomor urut baris itu **tidak berubah** |

Bila langkah 1 masih `409`, log kini mencatat `EmergencyVisitId` dan `SequenceDicoba` pada
entri `EmergencyTriage.Create`, sehingga sebabnya terbaca tanpa menebak.
