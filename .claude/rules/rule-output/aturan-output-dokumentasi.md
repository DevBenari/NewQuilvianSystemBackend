# Aturan Output Dokumentasi Quilvian

| Field | Nilai |
| --- | --- |
| Status | Canonical rule |
| Berlaku untuk | Semua skill pada `.claude/skills/` di backend dan frontend |
| Lokasi canonical | `NewQuilvianSystemBackend/.claude/rules/rule-output/aturan-output-dokumentasi.md` |
| Wajib dibaca sebelum | Menulis dokumen apa pun ke `docs/module-blueprints/` atau laporan handoff |

Aturan ini mengikat **bentuk dan gaya dokumen**. Aturan ini tidak mengubah gate approval,
batas kewenangan, atau prosedur yang tertulis pada masing-masing `SKILL.md`. Bila terjadi
pertentangan, aturan keamanan/privasi/invariant pada `SKILL.md` tetap menang.

## Ringkasan lima aturan

| No | Aturan | Inti |
| ---: | --- | --- |
| 1 | Bahasa Indonesia | Seluruh isi dokumen ditulis dalam Bahasa Indonesia |
| 2 | Mudah dipahami orang umum | Pembaca sasaran bukan hanya programmer |
| 3 | Detail dan bercontoh | Setiap hal yang sulit wajib diberi contoh konkret |
| 4 | Bisnis proses harus jelas | Alur kerja nyata dijelaskan urut dan lengkap |
| 5 | Endpoint bergaya Swagger | Tampilkan grup Swagger dan tabel API |

---

## Aturan 1 — Bahasa harus Bahasa Indonesia

Seluruh narasi, judul, tabel, keterangan gambar, dan kesimpulan ditulis dalam Bahasa
Indonesia.

Yang **tetap boleh** dibiarkan dalam bahasa aslinya karena mengubahnya justru membuat salah:

- nama teknis yang harus sama persis dengan kode: `MstAllowanceType`, `AllowanceTypeCode`,
  `IsRecurring`;
- path, route, dan nama file: `api/v1/corporate/human-resource/master-data/allowance-types`;
- istilah standar yang tidak punya padanan mapan: `endpoint`, `payload`, `commit`, `hash`,
  `idempotency`;
- status baku yang dipakai sistem: `READY`, `NOT_READY`, `DEV_DISCRETION`, `APPROVED`.

Bila memakai istilah asing, jelaskan sekali di kemunculan pertama.

Contoh benar:

> Sistem menolak permintaan ganda (*duplicate submit*), yaitu ketika pengguna menekan tombol
> Simpan dua kali sehingga data berpotensi tercatat dobel.

Contoh salah:

> The system rejects duplicate submit to prevent double record creation.

---

## Aturan 2 — Gunakan bahasa yang mudah dipahami orang umum

Pembaca dokumen ini mencakup pemilik proses bisnis, staf HR, perawat, admin, dan auditor —
bukan hanya developer. Tulis untuk mereka.

Cara praktis:

- Pakai kalimat pendek. Satu kalimat satu gagasan.
- Pakai kalimat aktif: "Petugas HR menyimpan data", bukan "Data disimpan oleh petugas HR".
- Ganti jargon teknis dengan padanan yang wajar bila memungkinkan.
- Jangan menyingkat tanpa penjelasan pada kemunculan pertama.

Tabel penggantian yang sering dipakai:

| Jangan tulis begini saja | Tulis begini |
| --- | --- |
| "Soft delete pada entity" | "Data tidak benar-benar dihapus, hanya ditandai tidak aktif sehingga masih bisa ditelusuri" |
| "Validasi unique constraint gagal" | "Kode tunjangan sudah dipakai data lain, jadi tidak bisa disimpan" |
| "Payload tidak sesuai schema" | "Isian yang dikirim tidak lengkap atau formatnya salah" |
| "Race condition saat update" | "Dua petugas mengubah data yang sama pada waktu hampir bersamaan" |
| "Endpoint mengembalikan 403" | "Pengguna tidak punya hak akses untuk tindakan ini (kode 403)" |

---

## Aturan 3 — Jelaskan secara detail beserta contoh

Setiap bagian yang berpotensi sulit dipahami wajib diikuti contoh konkret dengan angka atau
data nyata. Aturan tanpa contoh dianggap belum selesai ditulis.

Wajib diberi contoh:

- rumus dan perhitungan;
- aturan validasi dan batasannya;
- perubahan status;
- pesan error dan artinya bagi pengguna;
- kasus khusus, pembatalan, dan koreksi.

Contoh penulisan yang benar:

> **Aturan:** Tunjangan dengan metode perhitungan `Percentage` dihitung dari gaji pokok, dan
> hasilnya tidak boleh melebihi `MaximumAmount`.
>
> **Contoh:** Pegawai dengan gaji pokok Rp 5.000.000 mendapat tunjangan jabatan 10%.
> Perhitungannya 10% x Rp 5.000.000 = Rp 500.000. Karena batas maksimum yang disetel adalah
> Rp 400.000, maka yang dibayarkan tetap Rp 400.000, bukan Rp 500.000.

Contoh yang belum memenuhi aturan:

> Tunjangan persentase dihitung dari gaji pokok dengan batas maksimum.

---

## Aturan 4 — Bisnis proses harus jelas

Dokumen wajib menjelaskan proses bisnis nyata, bukan hanya daftar tabel dan endpoint.

Setiap proses bisnis minimal memuat:

1. **Tujuan** — hasil bisnis apa yang ingin dicapai.
2. **Pelaku** — siapa mengerjakan apa, dan siapa yang berwenang menyetujui.
3. **Pemicu** — kejadian yang memulai proses.
4. **Prasyarat** — data atau kondisi yang harus ada lebih dulu.
5. **Langkah utama** — urut, bernomor, satu langkah satu tindakan.
6. **Aturan bisnis** — larangan, batas, dan perhitungan yang berlaku.
7. **Perubahan status** — dari status apa ke status apa, dan siapa yang boleh memicunya.
8. **Jalur tidak normal** — pembatalan, koreksi, penolakan, data terlambat, sistem gagal.
9. **Hasil akhir** — kondisi data setelah proses selesai, dan siapa yang menerima dampaknya.

Sajikan perubahan status sebagai tabel agar tidak ambigu:

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `Draft` | Aktifkan | `Aktif` | Admin HR | Data wajib sudah lengkap |
| `Aktif` | Nonaktifkan | `Nonaktif` | Admin HR | Tidak sedang dipakai payroll berjalan |
| `Nonaktif` | Aktifkan kembali | `Aktif` | Admin HR | Masa berlaku belum lewat |

Jangan menggambarkan proses bisnis memakai flowchart atau use-case diagram. Gunakan langkah
bernomor, tabel pelaku, dan tabel perubahan status.

---

## Aturan 5 — Sajikan endpoint seperti tampilan Swagger

Pembaca harus bisa mencocokkan dokumen dengan halaman Swagger tanpa menebak.

### 5.1 Tulis grup Swagger sebagai judul

Gunakan nilai atribut `[Tags(...)]` pada controller, apa adanya, sebagai judul bagian API.

Contoh nyata dari
`Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Controllers/AllowanceTypeController.cs`:

```csharp
[Route("api/v1/corporate/human-resource/master-data/allowance-types")]
[Tags("Corporate / Human Resource / Master Data / Allowance Type")]
```

Maka judul bagian di dokumen ditulis:

```markdown
### Corporate / Human Resource / Master Data / Allowance Type

Base URL: `api/v1/corporate/human-resource/master-data/allowance-types`
```

Jika sebuah modul memakai beberapa controller, buat satu bagian per grup `[Tags(...)]`.

### 5.2 Sertakan tabel API

Setiap grup wajib punya tabel endpoint dengan kolom berikut:

| Kolom | Isi |
| --- | --- |
| Method | `GET`, `POST`, `PUT`, `PATCH`, atau `DELETE` |
| Path | Path relatif terhadap base URL, contoh `/{id}` atau `/summary` |
| Kegunaan | Satu kalimat dalam bahasa yang dipahami pengguna |
| Hak akses | Nilai `[AccessPermission(...)]`, contoh `AllowanceType : Read` |
| Request | Bentuk masukan: query, body DTO, atau `-` bila tidak ada |
| Response | Tipe data yang dikembalikan di dalam `ApiResponse<T>` |

Tulis kode status yang mungkin muncul beserta arti bisnisnya di bawah tabel, bukan hanya
angkanya.

### 5.3 Yang tidak boleh dilakukan

- Jangan menyalin seluruh isi Swagger mentah-mentah tanpa penjelasan kegunaan.
- Jangan menuliskan endpoint yang belum ada di kode sebagai seolah-olah sudah tersedia. Beri
  label jelas `Rencana (belum tersedia)` bila memang target desain.
- Jangan menampilkan token, password, atau data pasien/pegawai asli sebagai contoh. Gunakan
  data samaran.
- Jangan menampilkan UUID mentah sebagai satu-satunya penjelas; sertakan nama yang terbaca
  manusia.

---

## Checklist sebelum dokumen dianggap selesai

Periksa satu per satu:

1. Seluruh narasi berbahasa Indonesia.
2. Istilah asing yang dipakai sudah dijelaskan pada kemunculan pertama.
3. Setiap aturan, rumus, dan validasi punya contoh berangka.
4. Proses bisnis memuat tujuan, pelaku, pemicu, langkah, aturan, status, jalur tidak normal,
   dan hasil akhir.
5. Setiap grup endpoint memakai judul persis nilai `[Tags(...)]` dan punya tabel API lengkap.
6. Kode status dijelaskan artinya bagi pengguna.
7. Tidak ada data rahasia atau data asli pasien/pegawai di dalam contoh.
8. Klaim yang menyangkut kode disertai bukti `repository + path + line/symbol + commit SHA`
   sesuai aturan skill terkait.

Contoh lengkap penerapan seluruh aturan ada di
[contoh-dokumentasi-modul.md](contoh-dokumentasi-modul.md).
