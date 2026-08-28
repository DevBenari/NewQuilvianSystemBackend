# Template Laporan Task Backend

Dokumen ini menetapkan **bentuk** laporan task backend. **Lokasi**, waktu penulisan, dan batas
wewenangnya diatur `AGENTS.md` bagian *Pelaporan Task Modul*, dan bentuk di sini mengikuti aturan
output dokumentasi suite skill (`rules/rule-output/`) supaya keduanya tidak berbeda.

Catat bukti saja. Jangan pernah menuliskan password, token, connection string, key, atau nilai
konfigurasi sensitif ke dalam laporan.

---

## 1. Satu task, satu laporan, satu lokasi

Setiap task roadmap backend yang selesai dan sudah divalidasi menghasilkan **tepat satu** laporan
tracked:

```text
docs/module-blueprints/<module-slug>/task/report/backend/<TASK-ID>.md
```

Ketentuannya:

- Laporan tracked itu **satu-satunya** artefak laporan task. Jangan membuat laporan sesi, catatan
  handoff, issue log, atau salinan laporan di lokasi lain — termasuk di akar repository.
- Nama berkasnya adalah task ID itu sendiri, persis seperti roadmap — kapitalisasi, prefix, dan
  nomornya tidak diubah. Bila task dikerjakan ulang karena temuan baru,
  **perbarui berkas yang sama**; jangan membuat nama berkas baru.
- Urutannya tidak boleh dibalik:
  `implementasi → build/validasi → tulis laporan → perbarui roadmap dan requirement-traceability → baru boleh ditandai selesai`.

> **Ketentuan yang dicabut.** Versi terdahulu dokumen ini menyatakan template ini mengatur
> "handoff sesi yang tidak terlacak Git" yang berlaku bersamaan dengan laporan task tracked, lalu
> menunjuk jalur berkas yang sama untuk keduanya. Ketentuan itu **dicabut** karena bertabrakan
> dengan `AGENTS.md` bagian *Pelaporan Task Modul* dan dengan aturan output dokumentasi. Tidak ada
> laporan sesi. Yang ada hanya laporan tracked pada jalur di atas.

---

## 2. Gaya penulisan mengikuti aturan output dokumentasi

Laporan dibaca juga oleh orang di luar tim programmer, jadi lima aturan berikut berlaku penuh:

| No | Aturan | Yang harus dilakukan pada laporan |
| ---: | --- | --- |
| 1 | Bahasa Indonesia | Seluruh narasi, judul bagian, dan isi tabel ditulis dalam Bahasa Indonesia |
| 2 | Mudah dipahami orang umum | Jelaskan dampaknya bagi pengguna, bukan hanya nama kelas dan berkas |
| 3 | Detail dan bercontoh | Aturan, rumus, dan validasi yang rumit diberi contoh berangka |
| 4 | Proses bisnis jelas dan urut | Alur kerja nyata dijelaskan berurutan, termasuk jalur tidak normalnya |
| 5 | Endpoint bergaya Swagger | Judul grup memakai nilai `[Tags(...)]` persis, lalu tabel API |

Yang **tetap** ditulis dalam bentuk aslinya karena menerjemahkannya justru membuat salah:

- nama teknis, path, route, dan nama berkas — `MstEmergencyTriage`, `POST /{id}/retriage`;
- nilai status baku yang dibaca lintas repository sebagai kunci — `PASS`, `NEW ERROR`,
  `EXISTING / ENVIRONMENT ISSUE`, `NOT RUN`, `LIGHT`, `MEDIUM`, `HEAVY`, `EPIC`,
  `NOT APPLICABLE`, `NOT FEASIBLE`, `NOT REQUIRED`, `PROVIDED`, `NONE`.

Aturan lengkap beserta contoh penerapannya ada pada `rules/rule-output/aturan-output-dokumentasi.md`
di suite skill. Bila isinya berbeda dari ringkasan di atas, dokumen tersebut yang berlaku.

---

## 3. Kerangka laporan

```markdown
# Laporan Perubahan Backend — `<TASK-ID>`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `<TASK-ID>` |
| Judul | Judul task persis seperti roadmap |
| Slice | Slice roadmap yang dijawab task ini |
| Roadmap | Jalur berkas roadmap beserta bagiannya |
| Trace | ID keputusan, kontrak API, state matrix, dan validation matrix yang dirujuk |
| Contract version | Versi kontrak API yang berlaku, beserta status persetujuannya |
| Dependency | Task lain yang harus lebih dulu selesai, beserta statusnya |
| Klasifikasi | `LIGHT` / `MEDIUM` / `HEAVY` / `EPIC`, beserta rincian skornya |
| Task mode | Mode task yang berlaku, misalnya `BACKEND` atau `MODULE BLUEPRINT` |
| Target tulis | Repository dan jalur yang boleh ditulis pada task ini |
| Model | Model yang dipakai mengerjakan task |
| Commit backend saat dikerjakan | SHA commit |
| Tanggal | Tanggal penulisan laporan |
| Status | Kondisi sebenarnya di akhir pekerjaan |

---

## 1. Masalah yang diperbaiki

Keadaan sebelum perubahan dan akibat nyatanya bagi pengguna. Sertakan satu contoh konkret bila
masalahnya sulit dibayangkan.

---

## 2. Proses bisnis

Tujuan, pelaku, pemicu, langkah yang berurutan, aturan yang berlaku, status yang dihasilkan,
jalur tidak normal, dan hasil akhirnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Daftar berkas dan dokumen yang dibaca untuk menetapkan scope.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `<path>` | Apa yang berubah dan mengapa |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint atau payload yang berubah, atau `NOT APPLICABLE` |
| Database | Dampak schema, entity, atau migration, beserta status penerapannya |
| Keamanan/Auth | Dampak authorization, authentication, atau privasi, atau `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

Hanya bila task menyentuh endpoint. Judul grup memakai nilai `[Tags(...)]` persis seperti pada
controller.

#### <nilai [Tags(...)] persis seperti di controller>

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/retriage` | Kegunaannya bagi pengguna | `Resource : Action` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil tanpa error | `PASS` | Keluaran perintah |
| Skenario uji | Hasil sebenarnya | `PASS` / `NEW ERROR` / `EXISTING / ENVIRONMENT ISSUE` / `NOT RUN` | Bukti yang dapat ditelusuri |

Uji manual: `REQUIRED` / `PASS` / `FAIL` / `NOT FEASIBLE` / `NOT APPLICABLE`.

**Tidak dijalankan:** pemeriksaan yang sengaja tidak dijalankan beserta alasannya.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kriteria persis seperti roadmap | Terpenuhi / Belum terpenuhi | Bukti yang dapat ditelusuri |

Butir yang **belum** terpenuhi disebut apa adanya, bukan didiamkan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Peringatan yang muncul selama pengerjaan |
| Masalah yang diketahui | Masalah yang sengaja ditinggalkan beserta alasannya |
| Risiko tersisa | Risiko yang masih ada bila hasil ini dipakai sekarang |
| Perubahan sampingan | `NONE`, atau butir yang dipulihkan/dihapus beserta alasannya |
| Interupsi | `NONE`, atau jenis interupsi beserta pemulihan yang dilakukan |
| Status Git | Keluaran `git status --short` di akhir pekerjaan |
| Langkah berikutnya | Langkah yang disarankan |
```

Bagian yang tidak berlaku pada sebuah task tetap ditulis dengan nilai `NOT APPLICABLE` beserta
alasan singkatnya. Jangan menghapus barisnya diam-diam.

Untuk task `MODULE BLUEPRINT MODE`, tambahkan dua bagian: **status dan bukti blueprint**, serta
**bukti basi dan fase yang terblokir**. Bukti basi, fase yang terblokir, dan fase yang masih dapat
dilanjutkan secara mandiri dilaporkan sebagai tiga hal yang berbeda.

---

## 4. Checklist sebelum task ditandai selesai

- [ ] Build atau validasi yang diminta task benar-benar dijalankan, dan hasil sebenarnya dicatat
- [ ] Laporan ada di `docs/module-blueprints/<module-slug>/task/report/backend/` pada modul yang benar
- [ ] Nama berkas sama persis dengan task ID pada roadmap, contoh `BE-RWI-001.md`
- [ ] Seluruh narasi berbahasa Indonesia dan dapat dipahami pembaca non-programmer
- [ ] Proses bisnis dijelaskan urut, termasuk jalur tidak normalnya
- [ ] Setiap grup endpoint memakai judul `[Tags(...)]` persis dan punya tabel API
- [ ] Setiap acceptance criteria punya baris status beserta buktinya
- [ ] Butir DoD yang belum terpenuhi disebut apa adanya
- [ ] Status migration/database disebut eksplisit bila task menyentuh database
- [ ] Roadmap modul diberi tanda status dan tautan ke laporan
- [ ] `requirement-traceability.md` modul itu diperbarui buktinya
- [ ] Tidak ada credential, token, connection string, key, atau nilai konfigurasi sensitif di dalam laporan
