# Roadmap Frontend — Patient Management

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `blueprint_revision` | `5` — `approved` 16 Oktober 2026 (`PAT-DEC-068`). Riwayat: `4` (`PAT-DEC-067`), `3` (`PAT-DEC-033`), `2` |
| `roadmap_revision` | `2` — difinalkan 15 Oktober 2026, sesi ketiga, tetap tanpa task frontend. 14 Oktober 2026: mencatat bahwa `BE-PAT-MIG-002` juga diusulkan tanpa pekerjaan frontend; diamandemen 15 Oktober 2026 dengan hasil scan konsumen QR frontend; amandemen kedua 15 Oktober 2026 mencatat `PAT-DEC-060` dan dampak `BE-PAT-MIG-003`. Riwayat: `1` — dibuat 8 Oktober 2026, tidak berubah pada `blueprint_revision` `2` karena keputusan review `PAT-OQ-001`–`007` tidak menambah pekerjaan frontend |
| Status roadmap | `BE-PAT-MIG-001`: **`approved`** — tidak ada task frontend. `BE-PAT-MIG-002` dan `BE-PAT-MIG-003`: **`approved`** 15 Oktober 2026 (`PAT-DEC-033`) — tanpa task frontend (`PAT-DEC-060`) |
| Dasar | `PAT-DEC-015`: rekonsiliasi MRN Pilot adalah endpoint admin sementara tanpa layar. `PAT-DEC-060` (`OWNER_APPROVED`, menjawab `PAT-OQ-025`): worker migrasi massal juga endpoint admin tanpa layar; operator memakai admin API berwenang, Swagger, atau klien HTTP terkendali. Scan read-only 15 Oktober 2026 atas `QuilvianSystemFrontendDev` branch `fix/final-dev-tree-alignment` @ `2831ea63`: layar kiosk dan cetak kartu memakai `qrCodePath` dari respons API, dan kartu dasar serta IGD membuat QR sendiri dari MRN (`PAT-SRC-020`). Karena itu strategi QR yang disetujui (`PAT-DEC-052`, path `rsmmc-bulk`) **tidak** menuntut perubahan frontend. `BE-PAT-MIG-003` tidak mengubah bentuk request/response endpoint pendaftaran (kontrak B14.1), sehingga juga tidak menuntut task frontend |

Berkas ini sengaja dibuat walau kosong. Tanpanya, pembaca tidak dapat membedakan antara "memang
tidak ada pekerjaan frontend" dan "roadmap frontend terlupa ditulis".

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
(tidak ada task frontend pada roadmap_revision 2)
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| — | — | Tidak ada |

---

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| — | Tidak ada task frontend | `PAT-DEC-015` | — | — | — | — | — | — | — | — |

Bila kelak pemilik memutuskan rekonsiliasi atau kemampuan pasien lain butuh layar, task
frontendnya ditambahkan di sini lewat skill perencanaan, dengan keputusan baru yang tercatat.
`BE-PAT-MIG-001` tidak memberi wewenang membuat layar apa pun, dan begitu pula `BE-PAT-MIG-002`.

**Pengamatan untuk pemilik frontend, bukan task.** Sesudah `BE-PAT-MIG-003` dideploy ke Staging
dengan guard aktif, kode pasien normal baru berbentuk `PAT-RSMMC-705250` (16 karakter), satu karakter
lebih panjang dari bentuk sekarang seperti `PAT-RSMMC-00742` (15 karakter), dan MRN normal baru mulai
`00-89-01-88` dengan format yang sama. Kartu pasien dan layar kiosk yang menampilkan kode itu
sebaiknya diperiksa tata letaknya. Bentuk request dan respons sukses pendaftaran tidak berubah
(kontrak B14.1). Satu-satunya respons baru adalah penolakan `503` /
`PATIENT_RESERVATION_GUARD_CONFIG_INVALID` di Staging persis bila `Enabled` guard bukan `true` — `false`, hilang, atau tidak
sah (kontrak B14.3); layar pendaftaran dan kiosk menampilkannya lewat penanganan galat yang sudah
ada. Di Produksi tidak ada perubahan. *Teks lama: "Bila kelak guard reservasi ternyata mengubah
respons pendaftaran, dampak frontend-nya dinilai ulang pada revisi berikutnya."*
