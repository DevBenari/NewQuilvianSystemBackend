# rule-prascan — Wajib Scan Sistem Sebelum Wawancara Modul

Folder ini menyimpan aturan **gerbang masuk** workflow pengembangan modul: tidak ada wawancara
bisnis sebelum keadaan sistem yang sekarang dipetakan lebih dulu.

## Isi folder

| Berkas | Fungsi |
| --- | --- |
| [aturan-prascan-modul.md](aturan-prascan-modul.md) | Aturan wajib, gerbang `/grill-me`, Kartu Konteks Pra-Wawancara, dan kewajiban skill lain |
| [format-registry-sistem.md](format-registry-sistem.md) | Bentuk baku hasil pemindaian: tujuh berkas registry, legenda status `L0`–`L4`, dan larangan isi |

## Aturan dalam satu kalimat

> `/grill-me` berhenti sebelum pertanyaan pertama bila `docs/system-registry/` belum ada atau
> commit SHA-nya sudah tidak sama dengan `HEAD`.

## Mengapa

Backend Quilvian memuat 445 `DbSet` dan 246 controller. Wawancara yang dimulai tanpa peta
sistem menghasilkan modul yang membangun ulang tabel yang sudah ada, memakai nama yang sudah
dipakai, atau mengambil alih data milik modul lain. Ketiganya baru ketahuan saat integrasi,
ketika perbaikannya sudah mahal.

## Alur setelah aturan ini berlaku

```text
/qv-scan            → peta sistem apa adanya, sekali untuk semua modul
  ↓                   (wajib, dan wajib segar)
/qv-grill <modul>   → wawancara bisnis, dibuka dengan Kartu Konteks
  ↓
/qv-trace <modul>   → audit mendalam, hanya untuk modul tersebut
  ↓
/qv-design → /qv-plan → /qv-build-be + /qv-build-fe → /qv-verify
```

`/qv-scan` menjawab "sistem ini isinya apa". `/qv-trace` menjawab "kebutuhan modul ini sudah
tersedia atau belum". Keduanya berbeda dan tidak saling menggantikan.

## Pemeliharaan

- Ubah aturan hanya di sini, pada repository backend. Frontend membaca berkas ini langsung,
  tanpa salinan dan tanpa sidik jari SHA-256.
- Registry hasil pemindaian **bukan** isi folder ini. Registry tinggal di
  `docs/system-registry/` dan hanya boleh ditulis oleh `/scan-system-registry`.
- Aturan ini tidak menggantikan [rule-output](../rule-output/README.md). Registry tetap
  dokumen, sehingga tetap tunduk pada lima aturan output.
